namespace DomainManager.Requests;

public record UpdateSslMonitor {
    public string Domain { get; init; } = null!;
    public long ChatId { get; init; }
    public bool Delete { get; init; }
}