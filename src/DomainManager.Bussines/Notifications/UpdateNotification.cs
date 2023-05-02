using Telegram.Bot.Types;

namespace DomainManager.Notifications;

public record UpdateNotification {
    public Update Update { get; init; } = null!;
}