using DomainManager.Abstract;
using DomainManager.Notifications.CommandConsumers.Base;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
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
        if (args.Length is 0) {
            return await PingCommand(message, cancellationToken);
        }

        return args switch {
            [.., { } host] => await PingCommand(host, cancellationToken),
            _ => CommandHelpers.CommandAttributeByCommand[Command.PingCommand]!.Help!
        };
    }

    private async Task<string> PingCommand(Message message, CancellationToken cancellationToken) {

        Ping myPing = new Ping();
        PingReply reply = myPing.Send(message.Text, 1000);
        //return monitors.Count switch {
        //    0 => "Add your first host by `/ssl_monitor add my.site.com`.\n" +
        //         "Or `/ssl_monitor help` to get command help.",

        //    _ => "```\n" +
        //         "Expired on   | Host\n" +
        //         string.Join('\n', monitors) + "\n" +
        //         "```"
        //};
        return "sfasd";
    }
}