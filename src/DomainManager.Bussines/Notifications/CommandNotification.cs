using Telegram.Bot.Types;

namespace DomainManager.Notifications;

// ReSharper disable once ClassNeverInstantiated.Global
public record CommandNotification {
    public Command Command { get; init; }
    public string[]? Arguments { get; init; }
    public Message Message { get; init; } = null!;
}