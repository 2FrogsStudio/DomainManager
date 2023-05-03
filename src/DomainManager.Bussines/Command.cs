using DomainManager.Attributes;

namespace DomainManager;

public enum Command {
    Unknown,

    [Command("/help", Description = "Show this help")]
    Help,

    [Command("/domain_monitor", Description = "Monitor domain status")]
    DomainMonitor,

    [Command("/ssl_monitor", Description = "Monitor SSL certificate")]
    SslMonitor
}