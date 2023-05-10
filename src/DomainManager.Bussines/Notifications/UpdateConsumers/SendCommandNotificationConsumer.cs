using System.Diagnostics;
using DomainManager.Abstract;
using DomainManager.Requests;
using DomainManager.Services;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DomainManager.Notifications.UpdateConsumers;

public class SendCommandNotificationConsumer : IConsumer<UpdateNotification>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<SendCommandNotificationConsumer> _logger;
    private readonly IScopedMediator _mediator;
    private readonly IStaticService _staticService;

    public SendCommandNotificationConsumer(IScopedMediator mediator, IStaticService staticService,
        ILogger<SendCommandNotificationConsumer> logger, IHostEnvironment hostEnvironment,
        ITelegramBotClient botClient) {
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
            }) {
            return;
        }

        Command command;
        string[] args;
        if (messageText.StartsWith('/')) {
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
            command = CommandHelpers.CommandByText.TryGetValue(commandAndUserName[0], out var cmd)
                ? cmd
                : Command.Unknown;
            args = commandAndArgs.Length >= 2 ? commandAndArgs[1..] : Array.Empty<string>();
        } else {
            var response = await _mediator
                .CreateRequestClient<GetPipelineStateRequest>()
                .GetResponse<GetPipelineStateResponse, NoPipelineStateResponse>(new { }, cancellationToken);
            if (response.Is(out Response<NoPipelineStateResponse>? _)) {
                await _botClient.SendTextMessageAsync(chatId, "Not in pipeline context",
                    cancellationToken: cancellationToken);
                return;
            }
            if (response.Is(out Response<GetPipelineStateResponse>? pipelineState)) {
                command = pipelineState.Message.Command;
                args = messageText.Split(' ');
            } else {
                throw new UnreachableException();
            }
        }

        if (args.Length == 0 || (args.Length == 1 && args[0].Equals("help", StringComparison.OrdinalIgnoreCase))) {
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