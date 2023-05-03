using MassTransit;
using Telegram.Bot.Types;

namespace DomainManager.Notifications;

public abstract class CommandHandlerBase : IConsumer<CommandNotification> {
    private readonly Command _command;

    protected CommandHandlerBase(Command command) {
        _command = command;
    }

    public async Task Consume(ConsumeContext<CommandNotification> context) {
        if (context.Message.Command == _command) {
            await Consume(context.Message.Arguments, context.Message.Message, context.CancellationToken);
        }
    }

    protected abstract Task Consume(string[] args, Message message, CancellationToken cancellationToken);
}