using DomainManager.Abstract;
using DomainManager.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Whois;

namespace DomainManager.Requests;

public class UpdateDomainMonitorHandler : IConsumer<UpdateDomainMonitor>, IMediatorConsumer {
    private readonly ApplicationDbContext _db;
    private readonly TimeSpan _updateNoMoreThan = TimeSpan.FromHours(1);

    private readonly IWhoisLookup _whoisLookup;

    public UpdateDomainMonitorHandler(IWhoisLookup whoisLookup, ApplicationDbContext db) {
        _whoisLookup = whoisLookup;
        _db = db;
    }

    public async Task Consume(ConsumeContext<UpdateDomainMonitor> context) {
        var cancellationToken = context.CancellationToken;
        var domain = context.Message.Domain;
        var chatId = context.Message.ChatId;

        var entity = await _db.DomainMonitor.FirstOrDefaultAsync(
            d => d.Domain == domain,
            cancellationToken);

        if (context.Message.Delete) {
            await Delete(entity, context);
            return;
        }

        entity ??= new DomainMonitor { Domain = domain };
        if (entity.LastUpdateDate is null || DateTime.UtcNow - entity.LastUpdateDate >= _updateNoMoreThan) {
            WhoisResponse whois;
            try {
                whois = await _whoisLookup.LookupAsync(domain);
            } catch (Exception e) {
                await context.RespondAsync<MessageResponse>(new { e.Message });
                return;
            }

            entity.LastUpdateDate = DateTime.UtcNow;
            entity.ExpirationDate = whois.Expiration?.ToUniversalTime();
        }

        var updated = _db.DomainMonitor.Update(entity);

        if (!updated.IsKeySet) {
            await _db.SaveChangesAsync(cancellationToken);
        }

        if (await _db.DomainMonitorByChat.FindAsync(new object[] { chatId, entity.Id }, cancellationToken) is null) {
            await _db.DomainMonitorByChat.AddAsync(new DomainMonitorByChat {
                ChatId = chatId,
                DomainMonitorId = entity.Id
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        await context.RespondAsync(entity);
    }

    private async Task Delete(DomainMonitor? entity, ConsumeContext<UpdateDomainMonitor> context) {
        var cancellationToken = context.CancellationToken;
        var domain = context.Message.Domain;
        var chatId = context.Message.ChatId;

        if (entity is not null) {
            if (await _db.DomainMonitorByChat
                    .FindAsync(new object[] { chatId, entity.Id }, cancellationToken) is { } monitor) {
                _db.DomainMonitorByChat.Remove(monitor);
            }

            if (await _db.DomainMonitorByChat.CountAsync(domainMonitor =>
                        domainMonitor.DomainMonitorId == entity.Id,
                    cancellationToken) == 0) {
                _db.DomainMonitor.Remove(entity);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await context.RespondAsync<MessageResponse>(new { Message = "Okay. Domain has been deleted" });
        }

        await context.RespondAsync<MessageResponse>(new { Message = $"Domain `{domain}` was not found" });
    }
}