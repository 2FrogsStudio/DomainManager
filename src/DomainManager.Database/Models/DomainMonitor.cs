namespace DomainManager.Models;

public class DomainMonitor {
    public int Id { get; set; }
    public string Domain { get; set; }
    public DateTime? LastUpdate { get; set; }

    public ICollection<DomainMonitorByChat> DomainMonitors { get; set; } = null!;
}