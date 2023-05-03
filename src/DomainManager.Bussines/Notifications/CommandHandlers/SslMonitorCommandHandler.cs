using System.Text;
using DomainManager.Models;
using DomainManager.Requests;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DomainManager.Notifications.CommandHandlers;

public class SslMonitorCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;

    public SslMonitorCommandHandler(ITelegramBotClient botClient, ApplicationDbContext db, IScopedMediator mediator) :
        base(Command.SslMonitor) {
        _botClient = botClient;
        _db = db;
        _mediator = mediator;
    }

    protected override async Task Consume(string[] args, Message message, CancellationToken cancellationToken) {
        if (args.Length is 0) {
            var monitors = await _db.SslMonitorByChat
                .Where(m => m.ChatId == message.Chat.Id)
                .Select(m => m.SslMonitor)
                .Select(d => $"{d.Domain,-30} {d.NotAfter,17:g} {d.LastUpdateDate,17:g}")
                .ToListAsync(cancellationToken);

            var messageText = monitors.Count == 0
                ? "Add your first domain by `/ssl_monitor [domain]`"
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

        var response = await _mediator.CreateRequestClient<UpdateSslMonitor>()
            .GetResponse<SslMonitor, ErrorResponse>(new { ChatId = message.Chat.Id, Domain = domain },
                cancellationToken);
        if (response.Is(out Response<ErrorResponse>? error)) {
            await _botClient.SendTextMessageAsync(
                message.Chat.Id,
                error.Message.Message,
                ParseMode.Markdown,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken
            );
            return;
        }

        if (!response.Is(out Response<SslMonitor>? sslMonitorResponse)) {
            throw new InvalidOperationException();
        }

        var sslMonitor = sslMonitorResponse.Message;
        var text = new StringBuilder()
            .AppendLine($"Domain `{domain}` has been added to monitoring")
            .AppendLine()
            .AppendLine("```")
            .AppendLine($"{"Issued On:",-12} {sslMonitor.NotBefore}")
            .AppendLine($"{"Expires On:",-12} {sslMonitor.NotAfter}")
            .AppendLine($"{"Last update:",-12} {sslMonitor.LastUpdateDate}")
            .AppendLine($"{"Errors:",-12} {sslMonitor.Errors}")
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