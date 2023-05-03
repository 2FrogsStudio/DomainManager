namespace DomainManager.Models;

public class SslMonitorByChat {
    public long ChatId { get; set; }

    public int SslMonitorId { get; set; }
    public SslMonitor SslMonitor { get; set; }
}