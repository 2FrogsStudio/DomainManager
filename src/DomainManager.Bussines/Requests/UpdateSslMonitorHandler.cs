using DomainManager.Abstract;
using DomainManager.Models;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;

namespace DomainManager.Requests;

public class UpdateSslMonitorHandler : IConsumer<UpdateSslMonitor>, IMediatorConsumer {
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;
    private readonly TimeSpan _updateNoMoreThan = TimeSpan.FromHours(1);

    public UpdateSslMonitorHandler(ApplicationDbContext db, IScopedMediator mediator) {
        _db = db;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UpdateSslMonitor> context) {
        var cancellationToken = context.CancellationToken;
        var host = context.Message.Host;
        var chatId = context.Message.ChatId;

        var entity = await _db.SslMonitor.FirstOrDefaultAsync(
            d => d.Host == host,
            cancellationToken);

        if (context.Message.Delete) {
            await Delete(entity, context);
            return;
        }

        entity ??= new SslMonitor { Host = host };

        if (entity.LastUpdateDate is null || DateTime.UtcNow - entity.LastUpdateDate >= _updateNoMoreThan) {
            var response = await _mediator
                .CreateRequestClient<GetCertificateInfo>()
                .GetResponse<CertificateInfo, MessageResponse>(new { Hostname = host }, cancellationToken);
            if (response.Is(out Response<MessageResponse>? error)) {
                await context.RespondAsync(error.Message);
                return;
            }

            if (!response.Is(out Response<CertificateInfo>? certInfoResponse)) {
                throw new InvalidOperationException();
            }

            var certInfo = certInfoResponse.Message;
            entity.LastUpdateDate = DateTime.UtcNow;
            entity.Issuer = certInfo.Issuer;
            entity.NotAfter = certInfo.NotAfter.ToUniversalTime();
            entity.NotBefore = certInfo.NotBefore.ToUniversalTime();
            entity.Errors = certInfo.Errors;
        }

        var updated = _db.SslMonitor.Update(entity);
        if (!updated.IsKeySet) {
            await _db.SaveChangesAsync(cancellationToken);
        }

        var monitorByChat =
            await _db.SslMonitorByChat.FindAsync(new object[] { chatId, entity.Id }, cancellationToken);

        if (monitorByChat is null) {
            await _db.SslMonitorByChat.AddAsync(new SslMonitorByChat {
                ChatId = chatId,
                SslMonitorId = entity.Id
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        await context.RespondAsync(entity);
    }

    private async Task Delete(SslMonitor? entity, ConsumeContext<UpdateSslMonitor> context) {
        var cancellationToken = context.CancellationToken;
        var host = context.Message.Host;
        var chatId = context.Message.ChatId;

        if (entity is not null) {
            if (await _db.SslMonitorByChat
                    .FindAsync(new object[] { chatId, entity.Id }, cancellationToken) is { } monitor) {
                _db.SslMonitorByChat.Remove(monitor);
            }

            if (await _db.SslMonitorByChat.CountAsync(monitor =>
                        monitor.SslMonitorId == entity.Id,
                    cancellationToken) == 0) {
                _db.SslMonitor.Remove(entity);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await context.RespondAsync<MessageResponse>(new { Message = "Okay. Host has been deleted" });
        }

        await context.RespondAsync<MessageResponse>(new { Message = $"Host `{host}` was not found" });
    }
}