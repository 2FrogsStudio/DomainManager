using DomainManager.Abstract;
using MassTransit;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace DomainManager.Notifications.CommandConsumers;

public class StartCommandConsumer : IConsumer<CommandNotification>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;

    public StartCommandConsumer(ITelegramBotClient botClient) {
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<CommandNotification> context) {
        if (context.Message.Command != Command.Start) {
            return;
        }
        if (context.Message.Message is not { Chat.Id: var chatId, From.Id: var fromId }
            || chatId != fromId) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        var commandMenu = CommandHelpers.CommandAttributeByCommand
            .Where(pair => pair.Value is not null && pair.Value.InlineCommand)
            .Select(pair => new InlineKeyboardButton(pair.Key.ToString()) {
                CallbackData = pair.Key.ToString()
            })
            .ToArray();

        var commandRows = commandMenu.Split(3).ToArray();

        await _botClient.SendTextMessageAsync(chatId, "Main menu",
            replyMarkup: new InlineKeyboardMarkup(commandRows), cancellationToken: cancellationToken);
    }
}