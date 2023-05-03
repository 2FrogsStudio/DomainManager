using DomainManager.Attributes;

namespace DomainManager;

public enum Command {
    Unknown,

    [Command("/help", Description = "Show this help")]
    Help,

    [Command("/domain_monitor",
        Description = "Monitor domain status",
        Help = "```\n" +
               "/domain_monitor                   - get list of your domains to monitor\n" +
               "/domain_monitor help              - show this help\n" +
               "/domain_monitor domain.com        - add domain to monitoring\n" +
               "/domain_monitor domain.com remove - delete domain from monitoring\n" +
               "```")]
    DomainMonitor,

    [Command("/ssl_monitor",
        Description = "Monitor SSL certificate",
        Help = "```\n" +
               "/ssl_monitor                     - get list of your hosts to monitor\n" +
               "/ssl_monitor help                - show this help\n" +
               "/ssl_monitor my.site.com         - add host to monitoring\n" +
               "/ssl_monitor my.site.com remove  - delete host from monitoring\n" +
               "```")]
    SslMonitor
}