using DomainManager.Abstract;
using MassTransit;
using MassTransit.Mediator;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DomainManager.Notifications.UpdateConsumers;

public class SendCallbackNotificationConsumer : IConsumer<UpdateNotification>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;
    private readonly IScopedMediator _mediator;

    public SendCallbackNotificationConsumer(IScopedMediator mediator, ITelegramBotClient botClient) {
        _mediator = mediator;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<UpdateNotification> context) {
        var update = context.Message.Update;
        if (update is not {
                Type: UpdateType.CallbackQuery,
                CallbackQuery.Id: var callbackId
            }) {
            return;
        }

        var cancellationToken = context.CancellationToken;

        await _botClient.AnswerCallbackQueryAsync(callbackId, cancellationToken: context.CancellationToken);

        await _mediator.Send<CallbackNotification>(new {
            CallbackQuery = update.CallbackQuery!
        }, cancellationToken);
    }
}