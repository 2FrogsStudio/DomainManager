namespace DomainManager.Configuration;

public class BotOptions {
    public string Token { get; init; } = "YOUR_ACCESS_TOKEN_HERE"!;
    public long[] AdminGroupIds { get; init; } = Array.Empty<long>();
    public long[] AdminUserIds { get; init; } = Array.Empty<long>();
}