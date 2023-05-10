using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;

namespace DomainManager.Notifications.CommandConsumers;

public class PingCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    private readonly ILogger<PingCommandConsumer> _logger;

    public PingCommandConsumer(ITelegramBotClient botClient, IMemoryCache memoryCache,
        ILogger<PingCommandConsumer> logger) :
        base(Command.Ping, botClient, memoryCache) {
        _logger = logger;
    }

    protected override async Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {
        return args switch {
            [{ } host] => await PingCommand(host, cancellationToken),
            _ => CommandHelpers.CommandAttributeByCommand[Command.Ping]!.Help!
        };
    }

    private async Task<string> PingCommand(string host, CancellationToken cancellationToken) {
        _logger.LogInformation("Ping host: {Host}", host);
        var hostIps = await Dns.GetHostAddressesAsync(host, cancellationToken);
        _logger.LogInformation("Host: {Host} IPs: {HostIps}", host,
            JsonSerializer.Serialize(hostIps.Select(ip => ip.ToString())));
        if (hostIps is not [var hostIp, ..]) {
            return "IP Address was not resolved";
        }
        PingReply reply;
        using var ping = new Ping();
        try {
            reply = await ping.SendPingAsync(hostIp, 5000);
        } catch (PingException e) {
            return "Exception occurred: " + e.Message;
        }
        return $"Status :  {reply.Status:G} \n Latency : {reply.RoundtripTime}ms \n Address : {reply.Address}";
    }
}