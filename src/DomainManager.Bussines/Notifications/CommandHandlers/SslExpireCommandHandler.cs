using DomainManager.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandHandlers;

public class SslExpireCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _db;
    private readonly ISslUpdater _sslUpdater;

    public SslExpireCommandHandler(ITelegramBotClient botClient, ApplicationDbContext db, ISslUpdater sslUpdater) :
        base(Command.SslExpire) {
        _botClient = botClient;
        _db = db;
        _sslUpdater = sslUpdater;
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

        var certificateInfo = await _sslUpdater.GetCertificateInfo(domain, cancellationToken);

        await _botClient.SendTextMessageAsync(
            message.Chat.Id,
            certificateInfo,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken
        );
    }
}