namespace DomainManager.Models;

public class UserTokens {
    public long Id { get; set; }
    public string Secret { get; set; }

    public int ProviderId { get; set; }
    public Provider Provider { get; set; }
}