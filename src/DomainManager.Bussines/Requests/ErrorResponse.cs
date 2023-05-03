namespace DomainManager.Requests;

public record ErrorResponse {
    public string Message { get; init; }
}