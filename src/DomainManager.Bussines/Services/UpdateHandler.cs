using DomainManager.Notifications;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace DomainManager.Services;

public class UpdateHandler : IUpdateHandler {
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UpdateHandler(ILogger<UpdateHandler> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update,
        CancellationToken cancellationToken) {
        _logger.LogDebug("Update received: {@Update}", update);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IScopedMediator>();
        await mediator.Send<UpdateNotification>(new { Update = update }, cancellationToken);
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception,
        CancellationToken cancellationToken) {
        _logger.LogError(exception, "Telegram API Error");
        if (exception is RequestException) {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}