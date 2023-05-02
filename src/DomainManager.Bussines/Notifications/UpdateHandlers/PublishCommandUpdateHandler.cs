using DomainManager.Services;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;

namespace DomainManager.Notifications.UpdateHandlers;

public class PublishCommandUpdateHandler : IConsumer<UpdateNotification> {
    private readonly ILogger<PublishCommandUpdateHandler> _logger;
    private readonly IScopedMediator _mediator;
    private readonly IStaticService _staticService;


    public PublishCommandUpdateHandler(IScopedMediator mediator, IStaticService staticService,
        ILogger<PublishCommandUpdateHandler> logger) {
        _mediator = mediator;
        _staticService = staticService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateNotification> context) {
        var update = context.Message.Update;
        var cancellationToken = context.CancellationToken;

        if (update is not { Message.Text: { } messageText } ||
            !messageText.StartsWith('/')) {
            return;
        }

        var commandAndArgs = messageText.Split(' ');
        var commandAndUserName = commandAndArgs[0].Split('@', 2);
        if (commandAndUserName.Length == 2) {
            var botUsername = await _staticService.GetBotUsername(cancellationToken);
            if (commandAndUserName[1] != botUsername) {
                _logger.LogDebug(
                    "Command ignored die to wrong bot username Expected: {ExpectedUserName} Actual: {ActualUserName}",
                    botUsername, commandAndUserName[1]);
                return;
            }
        }

        var command = CommandHelpers.CommandByText.TryGetValue(commandAndUserName[0], out var cmd)
            ? cmd
            : Command.Unknown;
        var args = commandAndArgs.Length >= 2 ? commandAndArgs[1..] : null;

        await _mediator.Publish<CommandNotification>(new {
            Command = command,
            Arguments = args,
            update.Message
        }, cancellationToken);
    }
}