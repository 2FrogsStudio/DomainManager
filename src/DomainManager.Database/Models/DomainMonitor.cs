namespace DomainManager.Models;

public class DomainMonitor {
    public int Id { get; set; }
    public string Domain { get; set; } = null!;
    public DateTime? ExpirationDate { get; set; }
    public DateTime? LastUpdateDate { get; set; }

    public ICollection<DomainMonitorByChat> DomainMonitors { get; set; } = null!;
}