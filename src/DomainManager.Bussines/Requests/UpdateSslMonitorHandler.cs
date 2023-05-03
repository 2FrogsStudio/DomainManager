using DomainManager.Models;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;

namespace DomainManager.Requests;

public class UpdateSslMonitorHandler : IConsumer<UpdateSslMonitor> {
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;
    private readonly TimeSpan _updateNoMoreThan = TimeSpan.FromHours(1);

    public UpdateSslMonitorHandler(ApplicationDbContext db, IScopedMediator mediator) {
        _db = db;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UpdateSslMonitor> context) {
        var cancellationToken = context.CancellationToken;
        var domain = context.Message.Domain;
        var chatId = context.Message.ChatId;

        var entity = await _db.SslMonitor.FirstOrDefaultAsync(
                         d => d.Domain == domain,
                         cancellationToken)
                     ?? new SslMonitor { Domain = domain };

        if (entity.LastUpdateDate is null || DateTime.UtcNow - entity.LastUpdateDate >= _updateNoMoreThan) {
            var response = await _mediator
                .CreateRequestClient<GetCertificateInfo>()
                .GetResponse<CertificateInfo, ErrorResponse>(new { Hostname = domain }, cancellationToken);
            if (response.Is(out Response<ErrorResponse>? error)) {
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
}