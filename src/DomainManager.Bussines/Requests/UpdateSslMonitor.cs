namespace DomainManager.Requests;

public record UpdateSslMonitor {
    public string Domain { get; init; }
    public long ChatId { get; init; }
}