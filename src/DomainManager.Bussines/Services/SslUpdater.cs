using DomainManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainManager.Services;

public class SslUpdater : ISslUpdater {
    private readonly ApplicationDbContext _db;
    private readonly HttpClient _httpClient;

    public SslUpdater(ApplicationDbContext db, IHttpClientFactory clientFactory) {
        _db = db;
        _httpClient = clientFactory.CreateClient("CertificateExpiration");
    }

    public async Task<SslExpire> Update(string domain, CancellationToken cancellationToken) {
        var sslExpire = await _db.SslExpires.FirstOrDefaultAsync(
            expire => expire.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase),
            cancellationToken);

        // DateOnly expirationDate = await GetExpirationDate(domain,cancellationToken);
        //
        // if (sslExpire is null) {
        //     
        // }

        return new SslExpire();
    }

    public async Task<string> GetCertificateInfo(string domain, CancellationToken cancellationToken) {
        var requestUri = string.Concat(Uri.UriSchemeHttps, Uri.SchemeDelimiter, domain);

        var response =
            await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, requestUri), cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}