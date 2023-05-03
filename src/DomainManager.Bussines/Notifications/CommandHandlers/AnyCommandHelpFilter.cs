using MassTransit;
using Telegram.Bot;

namespace DomainManager.Notifications.CommandHandlers;

public class AnyCommandHelpFilter<T> : IFilter<SendContext<T>> where T : class {
    private readonly ITelegramBotClient _botClient;

    public AnyCommandHelpFilter(ITelegramBotClient botClient) {
        _botClient = botClient;
    }

    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next) {
        await next.Send(context);
    }

    public void Probe(ProbeContext context) { }
}