using DomainManager.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Whois;

namespace DomainManager.Requests;

public class UpdateDomainMonitorHandler : IConsumer<UpdateDomainMonitor> {
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
                         cancellationToken)
                     ?? new DomainMonitor { Domain = domain };

        if (entity.LastUpdateDate is null || DateTime.UtcNow - entity.LastUpdateDate >= _updateNoMoreThan) {
            WhoisResponse domainInfo;
            try {
                domainInfo = await _whoisLookup.LookupAsync(domain);
            } catch (Exception e) {
                await context.RespondAsync<ErrorResponse>(new { e.Message });
                return;
            }

            entity.LastUpdateDate = DateTime.UtcNow;
            entity.ExpirationDate = domainInfo.Expiration?.ToUniversalTime();
        }

        var updated = _db.DomainMonitor.Update(entity);
        if (!updated.IsKeySet) {
            await _db.SaveChangesAsync(cancellationToken);
        }

        var monitorByChat =
            await _db.DomainMonitorByChat.FindAsync(new object[] { chatId, entity.Id }, cancellationToken);

        if (monitorByChat is null) {
            await _db.DomainMonitorByChat.AddAsync(new DomainMonitorByChat {
                ChatId = chatId,
                DomainMonitorId = entity.Id
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        await context.RespondAsync(entity);
    }
}