using System.Diagnostics;
using DomainManager.Abstract;
using DomainManager.Models;
using DomainManager.Notifications.CommandConsumers.Base;
using DomainManager.Requests;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandConsumers;

public class SslMonitorCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;

    public SslMonitorCommandConsumer(ITelegramBotClient botClient, ApplicationDbContext db, IScopedMediator mediator,
        ILogger<SslMonitorCommandConsumer> logger) :
        base(Command.SslMonitor, botClient) {
        _db = db;
        _mediator = mediator;
    }

    protected override async Task<string> Consume(string[] args, Message message, CancellationToken cancellationToken) {
        if (args.Length is 0) {
            return await GetHostList(message, cancellationToken);
        }

        var chatId = message.Chat.Id;
        return args switch {
            ["list", ..] => await GetHostList(message, cancellationToken),
            ["delete", { } host] => await Update(chatId, host, true, cancellationToken),
            ["add", { } host] => await Update(chatId, host, false, cancellationToken),
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
            .Select(d => $"{d.Host,-30} {d.NotAfter,17:g} {d.LastUpdateDate,17:g}")
            .ToListAsync(cancellationToken);

        return monitors.Count switch {
            0 => "Add your first host by `/ssl_monitor add my.site.com`.\n" +
                 "Or `/ssl_monitor help` to get command help.",

            _ => "```\n" +
                 $"{"  Host",-30} {"Expired on   ",17} {"Last update   ",17}\n" +
                 string.Join('\n', monitors) + "\n" +
                 "```"
        };
    }
}