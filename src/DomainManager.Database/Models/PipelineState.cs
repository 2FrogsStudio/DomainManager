using System.ComponentModel.DataAnnotations;

namespace DomainManager.Models;

public class PipelineState {
    [Key] public string Command { get; set; } = null!;
}