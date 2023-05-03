using System.Text;
using DomainManager.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DomainManager.Notifications.CommandHandlers;

public class SslMonitorCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _db;
    private readonly ISslInfoUpdater _sslInfoUpdater;

    public SslMonitorCommandHandler(ITelegramBotClient botClient, ApplicationDbContext db,
        ISslInfoUpdater sslInfoUpdater) :
        base(Command.SslMonitor) {
        _botClient = botClient;
        _db = db;
        _sslInfoUpdater = sslInfoUpdater;
    }

    protected override async Task Consume(string[] args, Message message, CancellationToken cancellationToken) {
        if (args.Length is 0) {
            var sslMonitors = await _db.SslMonitorByChat
                .Where(m => m.ChatId == message.Chat.Id)
                .Select(m => m.SslMonitor)
                .Select(d => $"{d.Domain,-30} {d.NotAfter,17:g} {d.LastUpdateDate,17:g}")
                .ToListAsync(cancellationToken);

            var messageText = sslMonitors.Count == 0
                ? "Add your first domain by `/ssl_monitor [domain]`"
                : new StringBuilder()
                    .AppendLine("```")
                    .AppendLine($"{"Domain",-30} {"Expired on   ",17} {"Last update   ",17}")
                    .AppendLine(string.Join('\n', sslMonitors))
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

        var sslExpire = await _sslInfoUpdater.UpdateCertificateInfo(message.Chat.Id, domain, cancellationToken);

        var text = new StringBuilder()
            .AppendLine($"Domain `{domain}` has been added to monitoring")
            .AppendLine()
            .AppendLine("```")
            .AppendLine($"{"Issued On:",-12} {sslExpire.NotBefore}")
            .AppendLine($"{"Expires On:",-12} {sslExpire.NotAfter}")
            .AppendLine($"{"Last update:",-12} {sslExpire.LastUpdateDate}")
            .AppendLine($"{"Errors:",-12} {sslExpire.Errors}")
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