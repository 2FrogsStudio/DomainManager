using System.Net.Security;

namespace DomainManager.Models;

public class SslMonitor {
    public int Id { get; set; }
    public string Domain { get; set; }
    public DateTime? LastUpdateDate { get; set; }
    public string Issuer { get; set; }
    public DateTime NotAfter { get; set; }
    public DateTime NotBefore { get; set; }
    public SslPolicyErrors Errors { get; set; }

    public ICollection<SslMonitorByChat> SslMonitors { get; set; } = null!;
}