using System.Net.NetworkInformation;
using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;

namespace DomainManager.Notifications.CommandConsumers;

public class PingCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    public PingCommandConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache) :
        base(Command.PingCommand, botClient, memoryCache) { }

    protected override async Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {
        return args switch {
            [{ } host] => await PingCommand(host),
            _ => CommandHelpers.CommandAttributeByCommand[Command.PingCommand]!.Help!
        };
    }

    private static async Task<string> PingCommand(string host) {
        using var ping = new Ping();
        try {
            var reply = await ping.SendPingAsync(host, 5000);
            return $"Status :  {reply.Status:G} \n Latency : {reply.RoundtripTime}ms \n Address : {reply.Address}";
        } catch (PingException e) {
            return "Exception occurred: " + e.Message;
        }
    }
}