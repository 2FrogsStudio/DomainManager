namespace DomainManager.Requests;

public record UpdateDomainMonitor {
    public string Domain { get; init; } = null!;
    public long ChatId { get; init; }
    public bool Delete { get; init; }
}