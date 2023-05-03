using DomainManager.Attributes;

namespace DomainManager;

public enum Command {
    Unknown,

    [Command("/help", Description = "Show this help")]
    Help,

    [Command("/providers", Description = "Get supported providers")]
    Providers,

    [Command("/expiration", Description = "Get expiration date for domain e.g.`/whois google.com`")]
    Expiration
}