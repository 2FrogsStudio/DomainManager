namespace DomainManager.Notifications;

public record CallbackNotification {
    public CallbackQuery CallbackQuery { get; init; } = null!;
}