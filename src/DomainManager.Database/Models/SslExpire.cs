namespace DomainManager.Models;

public class SslExpire {
    public int Id { get; set; }
    public string Domain { get; set; }
    public DateOnly? ExpireDate { get; set; }
    public DateTime? LastUpdateDate { get; set; }
}