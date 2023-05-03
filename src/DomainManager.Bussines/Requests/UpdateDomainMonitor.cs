namespace DomainManager.Requests;

public record UpdateDomainMonitor {
    public string Domain { get; init; }
    public long ChatId { get; init; }
}