using DomainManager.Abstract;
using DomainManager.Configuration;
using DomainManager.Jobs;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DomainManager.Notifications.UpdateHandlers;

public class RunUpdateJobActivatorConsumer : IConsumer<UpdateNotification>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;
    private readonly IOptions<BotOptions> _botOptions;
    private readonly ILogger<RunUpdateJobActivatorConsumer> _logger;
    private readonly ISendEndpointProvider _sendEndpoint;

    public RunUpdateJobActivatorConsumer(ILogger<RunUpdateJobActivatorConsumer> logger,
        ITelegramBotClient botClient, IOptions<BotOptions> botOptions,
        Bind<ISecondBus, ISendEndpointProvider> sendEndpointProvider) {
        _logger = logger;
        _botClient = botClient;
        _botOptions = botOptions;
        _sendEndpoint = sendEndpointProvider.Value;
    }

    public async Task Consume(ConsumeContext<UpdateNotification> context) {
        var update = context.Message.Update;
        var cancellationToken = context.CancellationToken;

        if (update is not {
                Message : {
                    Text: { } messageText,
                    MessageId: var messageId,
                    Chat.Id: var chatId,
                    From.Id: var fromId
                }
            }
            || !_botOptions.Value.AdminUserIds.Contains(fromId)
            // can be private chat with admin
            || !(_botOptions.Value.AdminUserIds.Contains(chatId) || _botOptions.Value.AdminGroupIds.Contains(chatId))
            || !messageText.StartsWith("run_update")) {
            return;
        }

        var formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdateAndNotifyJobConsumer>();
        var endpoint = new Uri($"queue:{formatter}");
        var sendEndpoint = await _sendEndpoint.GetSendEndpoint(endpoint);
        await sendEndpoint.Send<UpdateAndNotifyJob>(new { }, cancellationToken);

        await _botClient.SendTextMessageAsync(
            chatId,
            "Update started..",
            ParseMode.Markdown,
            replyToMessageId: messageId,
            cancellationToken: context.CancellationToken
        );
    }
}