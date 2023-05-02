namespace DomainManager.Models;

public class UserTokens {
    public int Id { get; set; }
    public string Secret { get; set; }

    public int ProviderId { get; set; }
    public Provider Provider { get; set; }
}