namespace DomainManager.Requests;

public record GetCertificateInfo {
    public string Hostname { get; init; }
}