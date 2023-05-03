using DomainManager.Attributes;

namespace DomainManager;

public enum Command {
    Unknown,

    [Command("/help", Description = "Show this help")]
    Help,

    [Command("/domain_monitor",
        Description = "Monitor domain status",
        Help = "`/domain_monitor` - get list of your domains to monitor\n" +
               "`/domain_monitor help` - get this help\n" +
               "`/domain_monitor [domain]` - add domain to monitoring\n" +
               "`/domain_monitor remove [domain]` - delete domain from monitoring")]
    DomainMonitor,

    [Command("/ssl_monitor",
        Description = "Monitor SSL certificate",
        Help = "`/ssl_monitor` - get list of your hosts to monitor\n" +
               "`/ssl_monitor help` - get this help\n" +
               "`/ssl_monitor [host]` - add host to monitoring\n" +
               "`/ssl_monitor remove [host]` - delete host from monitoring")]
    SslMonitor
}