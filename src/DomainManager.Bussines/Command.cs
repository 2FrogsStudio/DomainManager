using DomainManager.Attributes;

namespace DomainManager;

public enum Command {
    Unknown,

    [Command("/help", Description = "Show this help")]
    Help,

    [Command("/providers", Description = "Get supported providers")]
    Providers,

    [Command("/whois", Description = "Get supported providers `/whois domain`")]
    Whois
}