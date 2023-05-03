using DomainManager.Models;

namespace DomainManager.Services;

public interface ISslInfoUpdater {
    Task<SslMonitor> UpdateCertificateInfo(long chatId, string domain, CancellationToken cancellationToken);
}