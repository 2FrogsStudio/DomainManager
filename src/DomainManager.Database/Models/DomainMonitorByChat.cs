namespace DomainManager.Models;

public class DomainMonitorByChat {
    public long ChatId { get; set; }

    public int DomainMonitorId { get; set; }
    public DomainMonitor DomainMonitor { get; set; }
}