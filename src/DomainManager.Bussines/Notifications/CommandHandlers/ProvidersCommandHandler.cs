using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DomainManager.Notifications.CommandHandlers;

public class ProvidersCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _db;

    public ProvidersCommandHandler(ITelegramBotClient botClient, ApplicationDbContext db) : base(Command.Providers) {
        _botClient = botClient;
        _db = db;
    }

    protected override async Task Consume(string[]? args, Message message, CancellationToken cancellationToken) {
        var sb = new StringBuilder()
            .AppendLine("Supported provider list:");
        await foreach (var provider in _db.Providers) sb.AppendLine(provider.Name);
        var text = sb.ToString();

        await _botClient.SendTextMessageAsync(
            message.Chat.Id,
            text,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }
}