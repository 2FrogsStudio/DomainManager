using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using MassTransit.Mediator;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;
using System.Net.NetworkInformation;

namespace DomainManager.Notifications.CommandConsumers;

public class PingCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    public PingCommandConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache) :
        base(Command.PingCommand, botClient, memoryCache) { }

    protected override async Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {
        return args switch {
            [.., { } host] => await PingCommand(host, cancellationToken),
            _ => CommandHelpers.CommandAttributeByCommand[Command.PingCommand]!.Help!
        };
    }

    private static async Task<string> PingCommand(string host) {
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