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
            [.., { } host, { } portStr] when isAdmin && int.TryParse(portStr, out var port)  => await ScanPort(host, port, cancellationToken),
            _ => CommandHelpers.CommandAttributeByCommand[Command.PortScan]!.Help!
        };
    }

    private static async Task<string> ScanPort(string host, int port, CancellationToken cancellationToken) {
        using var client = new TcpClient();
        bool open;
        try {
            await client.ConnectAsync(host, port, cancellationToken);
            open = true;
        } finally {
            client.Close();
        }
        return $"Port {port} on host {host} is {(open ? "open" : "closed")}";
    }
}