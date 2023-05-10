namespace DomainManager.Requests;

public record GetPipelineStateResponse {
    public Command Command { get; set; }
}