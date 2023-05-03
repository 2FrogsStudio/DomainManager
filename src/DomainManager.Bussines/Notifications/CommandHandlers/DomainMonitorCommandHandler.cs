using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Whois;

namespace DomainManager.Notifications.CommandHandlers;

public class DomainMonitorCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly IWhoisLookup _whoisLookup;

    public DomainMonitorCommandHandler(ITelegramBotClient botClient, IWhoisLookup whoisLookup) :
        base(Command.DomainMonitor) {
        _botClient = botClient;
        _whoisLookup = whoisLookup;
    }

    protected override async Task Consume(string[] args, Message message, CancellationToken cancellationToken) {
        if (args.Length is 0) {
            await _botClient.SendTextMessageAsync(
                message.Chat.Id,
                "No domain argument provided",
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken
            );
            return;
        }

        if (!args[0].TryGetDomainFromInput(out var domain)) {
            await _botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Domain format is not valid. Should be like google.com",
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken
            );
            return;
        }

        var response = await _whoisLookup.LookupAsync(domain);

        await _botClient.SendTextMessageAsync(
            message.Chat.Id,
            $"Expiration date: `{response.Expiration:d}`",
            ParseMode.Markdown,
            replyToMessageId: message.MessageId,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );
    }
}