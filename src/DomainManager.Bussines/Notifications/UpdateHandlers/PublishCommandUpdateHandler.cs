using DomainManager.Services;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DomainManager.Notifications.UpdateHandlers;

public class PublishCommandUpdateHandler : IConsumer<UpdateNotification> {
    private readonly ITelegramBotClient _botClient;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<PublishCommandUpdateHandler> _logger;
    private readonly IScopedMediator _mediator;
    private readonly IStaticService _staticService;


    public PublishCommandUpdateHandler(IScopedMediator mediator, IStaticService staticService,
        ILogger<PublishCommandUpdateHandler> logger, IHostEnvironment hostEnvironment, ITelegramBotClient botClient) {
        _mediator = mediator;
        _staticService = staticService;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<UpdateNotification> context) {
        var update = context.Message.Update;
        var cancellationToken = context.CancellationToken;

        if (update is not {
                Message : {
                    Text: { } messageText,
                    MessageId: var messageId,
                    Chat.Id: var chatId
                }
            } || !messageText.StartsWith('/')) {
            return;
        }

        var commandAndArgs = messageText.Split(' ');
        var commandAndUserName = commandAndArgs[0].Split('@', 2);
        switch (commandAndUserName.Length) {
            case 1 when update.Message.Chat.Type is not ChatType.Private && _hostEnvironment.IsDevelopment():
                return;
            case 2: {
                var botUsername = await _staticService.GetBotUsername(cancellationToken);
                if (commandAndUserName[1] != botUsername) {
                    _logger.LogDebug(
                        "Command ignored die to wrong bot username Expected: {ExpectedUserName} Actual: {ActualUserName}",
                        botUsername, commandAndUserName[1]);
                    return;
                }

                break;
            }
        }

        var command = CommandHelpers.CommandByText.TryGetValue(commandAndUserName[0], out var cmd)
            ? cmd
            : Command.Unknown;
        var args = commandAndArgs.Length >= 2 ? commandAndArgs[1..] : Array.Empty<string>();

        if (args.Length == 1 && args[0].Equals("help", StringComparison.OrdinalIgnoreCase)) {
            var help = CommandHelpers.CommandAttributeByCommand[command]?.Help;
            if (help is not null) {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    help,
                    ParseMode.Markdown,
                    replyToMessageId: messageId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: context.CancellationToken);
                return;
            }
        }

        await _mediator.Send<CommandNotification>(new {
            Command = command,
            Arguments = args,
            update.Message
        }, cancellationToken);
    }
}