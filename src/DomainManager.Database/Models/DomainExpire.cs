namespace DomainManager.Models;

public class DomainExpire {
    public long ChatId { get; set; }
    public string Domain { get; set; }
    public DateTime? LastUpdate { get; set; }
}