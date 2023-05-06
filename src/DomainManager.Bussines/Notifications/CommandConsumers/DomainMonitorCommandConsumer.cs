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
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandConsumers;

public class DomainMonitorCommandConsumer : CommandConsumerBase, IMediatorConsumer {
    private readonly ApplicationDbContext _db;
    private readonly IScopedMediator _mediator;

    public DomainMonitorCommandConsumer(ITelegramBotClient botClient, ApplicationDbContext db,
        IScopedMediator mediator, IMemoryCache memoryCache) :
        base(Command.DomainMonitor, botClient, memoryCache) {
        _db = db;
        _mediator = mediator;
    }

    protected override async Task<string> Consume(string[] args, Message message, long chatId1, bool isAdmin,
        CancellationToken cancellationToken) {
        if (args.Length is 0) {
            return await GetDomainList(message, cancellationToken);
        }

        var chatId = message.Chat.Id;
        return args switch {
            ["list", ..] => await GetDomainList(message, cancellationToken),
            ["add", { } host] when isAdmin => await Update(chatId, host, false, cancellationToken),
            ["add", not null] => "You have no access to add host",
            ["delete", { } host] when isAdmin => await Update(chatId, host, true, cancellationToken),
            ["delete", not null] => "You have no access to delete host",
            _ => CommandHelpers.CommandAttributeByCommand[Command.SslMonitor]!.Help!
        };
    }

    private async Task<string> Update(object chatId, string domain, bool delete, CancellationToken cancellationToken) {
        var response = await _mediator.CreateRequestClient<UpdateDomainMonitor>()
            .GetResponse<DomainMonitor, MessageResponse>(new {
                Domain = domain,
                ChatId = chatId,
                Delete = delete
            }, cancellationToken);

        if (response.Is(out Response<DomainMonitor>? successResponse)) {
            var domainMonitor = successResponse!.Message;

            return $"Domain `{domain}` has been added to monitoring\n" +
                   $"\n" +
                   $"```\n" +
                   $"Expired on:  {domainMonitor.ExpirationDate}\n" +
                   $"Last update: {domainMonitor.LastUpdateDate}\n" +
                   $"```";
        }

        if (response.Is(out Response<MessageResponse>? error)) {
            return error.Message.Message;
        }

        throw new UnreachableException();
    }

    private async Task<string> GetDomainList(Message message, CancellationToken cancellationToken) {
        var monitors = await _db.DomainMonitorByChat
            .Where(m => m.ChatId == message.Chat.Id)
            .Select(m => m.DomainMonitor)
            .OrderBy(m => m.ExpirationDate)
            .Select(d => $"{d.ExpirationDate,12:d} | {d.Domain}")
            .ToListAsync(cancellationToken);

        return monitors.Count switch {
            0 => "Add your first domain to monitor by `/domain_monitor add domain.com`.\n" +
                 "Or `/domain_monitor help` to get command help.",

            _ => "```\n" +
                 "Expired on   | Domain\n" +
                 string.Join('\n', monitors) + "\n" +
                 "```"
        };
    }
}