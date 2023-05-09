using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using MassTransit.Mediator;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;
using System.Net.NetworkInformation;

namespace DomainManager.Notifications.CommandConsumers;

public class PingCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;

    public PingCommandConsumer(ITelegramBotClient botClient, ApplicationDbContext db, IScopedMediator mediator,
        IMemoryCache memoryCache) :
        base(Command.PingCommand, botClient, memoryCache) {
        _db = db;
        _mediator = mediator;
    }

    protected override async Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {
        return args switch {
            [.., { } host] => await PingCommand(host, cancellationToken),
            _ => CommandHelpers.CommandAttributeByCommand[Command.PingCommand]!.Help!
        };
    }

    private async Task<string> PingCommand(string host, CancellationToken cancellationToken) {
        Ping myPing = new Ping();
        PingReply reply = myPing.Send(host, 5000);
        try
        {
            if (reply == null) return "Ping timeout";
            return "Status :  " + reply.Status + " \n Latency : " + reply.RoundtripTime.ToString() + "ms \n Address : " + reply.Address;
        }
        catch (PingException e)
        {
            return "Exception occurred: " + e.Message;
        }
        
    }
}