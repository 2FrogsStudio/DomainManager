using System.Net.Sockets;
using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using MassTransit.Mediator;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;

namespace DomainManager.Notifications.CommandConsumers;

public class PortScanner : CommandConsumerBase, IMediatorConsumer {
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;

    public PortScanner(ITelegramBotClient botClient, ApplicationDbContext db, IScopedMediator mediator,
        IMemoryCache memoryCache) :
        base(Command.PortScan, botClient, memoryCache) {
        _db = db;
        _mediator = mediator;
    }

    protected override async Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {

        return args switch {
            [.., { } host, { } port] when isAdmin => await GetHostList(host, port, cancellationToken),
            _ => CommandHelpers.CommandAttributeByCommand[Command.PortScan]!.Help!
        };
    }

    private async Task<string> GetHostList(string host, string port, CancellationToken cancellationToken) {
        TcpClient client = new TcpClient();
        try {
            await client.ConnectAsync(host, int.Parse(port), cancellationToken);
            return "Port " + port + " on host " + host + " is open";
        }
        catch (SocketException) {
            return "Port " + port + " on host " + host + " is closed";
        }
        finally {
            client.Close();
        }
    }
}