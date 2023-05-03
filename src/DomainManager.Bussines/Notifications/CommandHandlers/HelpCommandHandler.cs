using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DomainManager.Notifications.CommandHandlers;

public class HelpCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;

    public HelpCommandHandler(ITelegramBotClient botClient) : base(Command.Help) {
        _botClient = botClient;
    }

    protected override async Task Consume(string[] args, Message message, CancellationToken cancellationToken) {
        var sb = new StringBuilder("Usage:\n");
        foreach (var (_, commandDescription) in
                 CommandHelpers.CommandAttributeByCommand.Where(c => c.Value is not null))
            sb.Append($"{commandDescription!.Text}\t- {commandDescription.Description}\n");
        var text = sb.ToString().TrimEnd('\n');

        await _botClient.SendTextMessageAsync(
            message.Chat.Id,
            text,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }
}