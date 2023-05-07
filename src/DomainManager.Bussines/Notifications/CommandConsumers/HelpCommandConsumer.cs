using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;

namespace DomainManager.Notifications.CommandConsumers;

public class HelpCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    public HelpCommandConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache) : base(Command.Help, botClient,
        memoryCache) { }

    protected override Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {
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