using System.Diagnostics;
using DomainManager.Abstract;
using DomainManager.Models;
using DomainManager.Notifications.CommandConsumers.Base;
using DomainManager.Requests;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;

namespace DomainManager.Notifications.CommandConsumers;

public class SslMonitorCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;

    public SslMonitorCommandConsumer(ITelegramBotClient botClient, ApplicationDbContext db, IScopedMediator mediator,
        IMemoryCache memoryCache) :
        base(Command.SslMonitor, botClient, memoryCache) {
        _db = db;
        _mediator = mediator;
    }

    protected override async Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {
        if (args.Length is 0) {
            return await GetHostList(message, cancellationToken);
        }

        return args switch {
            ["list", ..] => await GetHostList(message, cancellationToken),
            ["add", { } host] when isAdmin => await Update(chatId, host, false, cancellationToken),
            ["add", not null] => "You have not access to add domains",
            ["delete", { } host] when isAdmin => await Update(chatId, host, true, cancellationToken),
            ["delete", not null] => "You have not access to delete domains",
            _ => CommandHelpers.CommandAttributeByCommand[Command.SslMonitor]!.Help!
        };
    }

    private async Task<string> Update(long chatId, string host, bool delete, CancellationToken cancellationToken) {
        var response = await _mediator.CreateRequestClient<UpdateSslMonitor>()
            .GetResponse<SslMonitor, MessageResponse>(new {
                    ChatId = chatId,
                    Host = host,
                    Delete = delete
                },
                cancellationToken);
        if (response.Is(out Response<SslMonitor>? sslMonitorResponse)) {
            var sslMonitor = sslMonitorResponse.Message;
            return $"Host `{host}` has been added to monitoring\n" +
                   $"\n" +
                   $"```\n" +
                   $"Expired on:  {sslMonitor.NotBefore}\n" +
                   $"Last update: {sslMonitor.LastUpdateDate}\n" +
                   $"```";
        }
        if (response.Is(out Response<MessageResponse>? error)) {
            return error.Message.Message;
        }
        throw new UnreachableException();
    }

    private async Task<string> GetHostList(Message message, CancellationToken cancellationToken) {
        var monitors = await _db.SslMonitorByChat
            .Where(m => m.ChatId == message.Chat.Id)
            .Select(m => m.SslMonitor)
            .OrderBy(m => m.NotAfter)
            .Select(d => $"{d.NotAfter,12:d} | {d.Host}")
            .ToListAsync(cancellationToken);

        return monitors.Count switch {
            0 => "Add your first host by `/ssl_monitor add my.site.com`.\n" +
                 "Or `/ssl_monitor help` to get command help.",

            _ => "```\n" +
                 "Expired on   | Host\n" +
                 string.Join('\n', monitors) + "\n" +
                 "```"
        };
    }
}