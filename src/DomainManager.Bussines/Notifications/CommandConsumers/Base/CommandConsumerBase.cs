using MassTransit;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DomainManager.Notifications.CommandConsumers.Base;

public abstract class CommandConsumerBase : IConsumer<CommandNotification> {
    private readonly ITelegramBotClient _botClient;
    private readonly Command _command;

    protected CommandConsumerBase(Command command, ITelegramBotClient botClient) {
        _command = command;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<CommandNotification> context) {
        if (context.Message.Command == _command) {
            var message = context.Message.Message;
            var replyText = await Consume(context.Message.Arguments, context.Message.Message,
                context.CancellationToken);
            await _botClient.SendTextMessageAsync(
                message.Chat.Id,
                replyText,
                ParseMode.Markdown,
                replyToMessageId: message.MessageId,
                cancellationToken: context.CancellationToken
            );
        }
    }

    protected abstract Task<string> Consume(string[] args, Message message, CancellationToken cancellationToken);
}