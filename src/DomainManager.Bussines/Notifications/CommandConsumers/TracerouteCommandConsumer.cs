using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using MassTransit.Mediator;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;
using System.Net.NetworkInformation;

namespace DomainManager.Notifications.CommandConsumers;

public class TracerouteCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    public TracerouteCommandConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache) :
        base(Command.TracerouteCommand, botClient, memoryCache) { }

    protected override async Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {
        return args switch {
            [.., { } host] => await TracerouteCommand(host),
            _ => CommandHelpers.CommandAttributeByCommand[Command.TracerouteCommand]!.Help!
        };
    }

    private static async Task<string> TracerouteCommand(string host) {
        var result = "";
        using var pingSender = new Ping();
        for (var ttl = 1; ttl <= 30; ttl++) {
            var options = new PingOptions(ttl, true);
            var reply = await pingSender.SendPingAsync(host, 5000, new byte[32], options);

            result += $"{ttl.ToString(),3} ";

            if (reply.Status is IPStatus.TtlExpired or IPStatus.Success) {
                result += $"{reply.Address,16} {reply.RoundtripTime}ms\n";

                if (reply.Status == IPStatus.Success)
                    break;
            } else {
                result += "* * * * Request timed out.\n";
            }
        }
        return result;
    }
}