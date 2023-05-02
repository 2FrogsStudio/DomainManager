using DomainManager.Attributes;

namespace DomainManager;

public enum Command {
    Unknown,

    [Command("/help", Description = "Show this help")]
    Help
}