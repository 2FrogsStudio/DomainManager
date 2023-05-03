using System.Text;
using DomainManager.Models;
using DomainManager.Requests;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Whois;

namespace DomainManager.Notifications.CommandHandlers;

public class DomainMonitorCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;
    private readonly IWhoisLookup _whoisLookup;

    public DomainMonitorCommandHandler(ITelegramBotClient botClient, IWhoisLookup whoisLookup, ApplicationDbContext db,
        IScopedMediator mediator) :
        base(Command.DomainMonitor) {
        _botClient = botClient;
        _whoisLookup = whoisLookup;
        _db = db;
        _mediator = mediator;
    }

    protected override async Task Consume(string[] args, Message message, CancellationToken cancellationToken) {
        if (args.Length is 0) {
            var monitors = await _db.DomainMonitorByChat
                .Where(m => m.ChatId == message.Chat.Id)
                .Select(m => m.DomainMonitor)
                .Select(d => $"{d.Domain,-30} {d.ExpirationDate,17:g} {d.LastUpdateDate,17:g}")
                .ToListAsync(cancellationToken);

            var messageText = monitors.Count == 0
                ? "Add your first domain by `/domain_monitor [domain]`"
                : new StringBuilder()
                    .AppendLine("```")
                    .AppendLine($"{"Domain",-30} {"Expired on   ",17} {"Last update   ",17}")
                    .AppendLine(string.Join('\n', monitors))
                    .AppendLine("```")
                    .ToString();

            await _botClient.SendTextMessageAsync(
                message.Chat.Id,
                messageText,
                ParseMode.Markdown,
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

        var response = await _mediator.CreateRequestClient<UpdateDomainMonitor>()
            .GetResponse<DomainMonitor>(new {
                Domain = domain,
                ChatId = message.Chat.Id
            }, cancellationToken);
        var domainMonitor = response.Message;

        var text = new StringBuilder()
            .AppendLine($"Domain `{domain}` has been added to monitoring")
            .AppendLine()
            .AppendLine("```")
            .AppendLine($"{"Expires On:",-12} {domainMonitor.ExpirationDate}")
            .AppendLine($"{"Last update:",-12} {domainMonitor.LastUpdateDate}")
            .AppendLine("```")
            .ToString();

        await _botClient.SendTextMessageAsync(
            message.Chat.Id,
            text,
            ParseMode.Markdown,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken
        );
    }
}