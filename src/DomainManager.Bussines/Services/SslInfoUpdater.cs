using DomainManager.Models;
using DomainManager.Requests;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;

namespace DomainManager.Services;

public class SslInfoUpdater : ISslInfoUpdater {
    private readonly ApplicationDbContext _db;
    private readonly IMediator _mediator;
    private readonly TimeSpan _updateNoMoreThan = TimeSpan.FromHours(1);

    public SslInfoUpdater(ApplicationDbContext db, IMediator mediator) {
        _db = db;
        _mediator = mediator;
    }

    public async Task<SslMonitor>
        UpdateCertificateInfo(long chatId, string domain, CancellationToken cancellationToken) {
        var entity = await _db.SslMonitor.FirstOrDefaultAsync(
                         d => d.Domain == domain,
                         cancellationToken)
                     ?? new SslMonitor { Domain = domain };

        if (entity.LastUpdateDate is null || DateTime.UtcNow - entity.LastUpdateDate >= _updateNoMoreThan) {
            var response = await _mediator
                .CreateRequestClient<GetCertificateInfo>()
                .GetResponse<CertificateInfo>(new { Hostname = domain }, cancellationToken);

            var certInfo = response.Message;

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

        var sslMonitorByChat =
            await _db.SslMonitorByChat.FindAsync(new object[] { chatId, entity.Id }, cancellationToken);

        if (sslMonitorByChat is null) {
            await _db.SslMonitorByChat.AddAsync(new SslMonitorByChat {
                ChatId = chatId,
                SslMonitorId = entity.Id
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return entity;
    }
}