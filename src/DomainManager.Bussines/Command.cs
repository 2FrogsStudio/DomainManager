using DomainManager.Attributes;

namespace DomainManager;

public enum Command {
    Unknown,

    [Command("/help", Description = "Show this help")]
    Help,

    [Command("/domain_monitor",
        Description = "Monitor domain status",
        Help = "`/domain_monitor help` - show this help\n" +
               "`/domain_monitor list` - get list of your domains to monitor\n" +
               "`/domain_monitor add` `domain.com` - add domain to monitoring\n" +
               "`/domain_monitor delete` `domain.com` - delete domain from monitoring\n" +
               "  [*] to add or delete domain you should be a chat admins"
    )]
    DomainMonitor,

    [Command("/ssl_monitor",
        Description = "Monitor SSL certificate",
        Help = "`/ssl_monitor help` - show this help\n" +
               "`/ssl_monitor list` - get list of your hosts to monitor\n" +
               "`/ssl_monitor add` `my.site.com` - add host to monitoring\n" +
               "`/ssl_monitor delete` `my.site.com` - delete host from monitoring\n" +
               "  [*] to add or delete host you should be a chat admins"
    )]
    SslMonitor,

    [Command("/schedule",
        Description = "Schedule monitoring job",
        Help = "`/schedule help` - show this help\n" +
               "`/schedule` [cron_expr] - enable monitoring job\n" +
               "`/schedule off` - disable monitoring job\n" +
               "`/schedule run` - force run updating job\n" +
               "`/schedule status` - get job status\n" +
               "  [cron_expr] `0 0 12 ? * 2-6 *` - fire monitoring job every 12 hours from monday to friday\n" +
               "  [*] to control job you should be a chat admins"
    )]
    Schedule
}