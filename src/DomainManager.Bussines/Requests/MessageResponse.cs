namespace DomainManager.Requests;

public record MessageResponse {
    public string Message { get; init; } = null!;
}