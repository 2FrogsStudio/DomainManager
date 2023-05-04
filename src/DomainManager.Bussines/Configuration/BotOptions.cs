namespace DomainManager.Configuration;

public class BotOptions {
    public string Token { get; init; }
    public long[] AdminGroupIds { get; init; }
    public long[] AdminUserIds { get; init; }
}