using DomainManager.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandHandlers;

public class HelpCommandHandler : CommandHandlerBase, IMediatorConsumer {
    public HelpCommandHandler(ITelegramBotClient botClient) : base(Command.Help, botClient) { }

    protected override Task<string> Consume(string[] args, Message message, CancellationToken cancellationToken) {
        var commandHelps =
            CommandHelpers.CommandAttributeByCommand
                .Where(c => c.Value is not null)
                .Select(c => $"{c.Value!.Text} - {c.Value.Description}");

        var text =
            "Usage:\n" +
            string.Join('\n', commandHelps);

        return Task.FromResult(text.Replace("_", @"\_"));
    }
}