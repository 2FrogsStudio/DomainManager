using DomainManager.Models;

namespace DomainManager.Services;

public interface ISslUpdater {
    public Task<SslExpire> Update(string domain, CancellationToken cancellationToken);
    Task<string> GetCertificateInfo(string domain, CancellationToken cancellationToken);
}