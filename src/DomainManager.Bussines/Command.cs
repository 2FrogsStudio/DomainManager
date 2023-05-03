using DomainManager.Attributes;

namespace DomainManager;

public enum Command {
    Unknown,

    [Command("/help", Description = "Show this help")]
    Help,

    [Command("/providers", Description = "Get supported providers")]
    Providers,

    [Command("/domain_expire", Description = "Monitor domain expiration e.g.`/domain_expire google.com`")]
    DomainExpire,

    [Command("/ssl_expire", Description = "Monitor SSL expiration e.g.`/ssl_expire google.com`")]
    SslExpire
}