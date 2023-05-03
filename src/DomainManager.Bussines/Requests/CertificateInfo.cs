using System.Net.Security;

namespace DomainManager.Requests;

public record CertificateInfo {
    public string? Issuer { get; set; }
    public DateTime NotAfter { get; set; }
    public DateTime NotBefore { get; set; }
    public SslPolicyErrors Errors { get; set; }
}