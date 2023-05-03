using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Whois;

namespace DomainManager.Notifications.CommandHandlers;

public partial class WhoisCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly IWhoisLookup _whoisLookup;

    public WhoisCommandHandler(ITelegramBotClient botClient, IWhoisLookup whoisLookup) :
        base(Command.Whois) {
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

        if (!TryGetDomainFromInput(args[0], out var domain)) {
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
            $"```{(response.ContentLength >= 4000 ? response.Content[..4000] + "..." : response.Content)}```",
            ParseMode.Markdown,
            replyToMessageId: message.MessageId,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );
    }


    private static bool TryGetDomainFromInput(string input, out string domain) {
        if (!input.Contains(Uri.SchemeDelimiter)) {
            input = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, input);
        }

        domain = new Uri(input).Host;
        return true;
    }

    [GeneratedRegex("/^[a-zA-Z0-9][a-zA-Z0-9-]{1,61}[a-zA-Z0-9]\\.[a-zA-Z]{2,}$/", RegexOptions.Compiled)]
    private static partial Regex DomainRegex();
}