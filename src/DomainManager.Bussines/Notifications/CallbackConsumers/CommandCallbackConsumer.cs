using DomainManager.Abstract;
using DomainManager.Commands;
using MassTransit;
using MassTransit.Mediator;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace DomainManager.Notifications.CallbackConsumers;

public class CommandCallbackConsumer : IConsumer<CallbackNotification>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;
    private readonly IScopedMediator _mediator;

    public CommandCallbackConsumer(ITelegramBotClient botClient, IScopedMediator mediator) {
        _botClient = botClient;
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CallbackNotification> context) {
        var callback = context.Message.CallbackQuery;
        if (callback is not {
                Data: var data,
                Message.Chat.Id: var chatId
            } || !Enum.TryParse(data, out Command command)) {
            return;
        }

        await _mediator.Send<SetPipelineStateCommand>(new {
            Command = command
        }, context.CancellationToken);

        await _botClient.SendTextMessageAsync(chatId,
            "Ok. Send me the args for command:",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: context.CancellationToken);
    }
}