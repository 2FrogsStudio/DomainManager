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
            [.., { } host] => await TracerouteCommand(host, cancellationToken),
            _ => CommandHelpers.CommandAttributeByCommand[Command.TracerouteCommand]!.Help!
        };
    }

    private async Task<string> TracerouteCommand(string host, CancellationToken cancellationToken) {
        var result = "";
        using (Ping pingSender = new Ping())
        {
            for (int ttl = 1; ttl <= 30; ttl++)
            {
                PingOptions options = new PingOptions(ttl, true);
                PingReply reply = pingSender.Send(host, 5000, new byte[32], options);

                result += ttl.ToString().PadLeft(3) + " ";

                if (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.Success)
                {
                    result += reply.Address.ToString().PadLeft(16) + " " + reply.RoundtripTime + "ms\n";

                    if (reply.Status == IPStatus.Success)
                        break;
                }
                else
                {
                    result += "* * * * Request timed out.\n";
                }
            }
        }
        return result;
    }
}