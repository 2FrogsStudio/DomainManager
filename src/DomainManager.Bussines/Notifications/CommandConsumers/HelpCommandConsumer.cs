using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandConsumers;

public class HelpCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    public HelpCommandConsumer(ITelegramBotClient botClient) : base(Command.Help, botClient) { }

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