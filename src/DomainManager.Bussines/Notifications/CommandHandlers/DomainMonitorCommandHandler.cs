using DomainManager.Abstract;
using DomainManager.Models;
using DomainManager.Requests;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandHandlers;

public class DomainMonitorCommandHandler : CommandHandlerBase, IMediatorConsumer {
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DomainMonitorCommandHandler> _logger;
    private readonly IScopedMediator _mediator;

    public DomainMonitorCommandHandler(ITelegramBotClient botClient, ApplicationDbContext db,
        IScopedMediator mediator, ILogger<DomainMonitorCommandHandler> logger) :
        base(Command.DomainMonitor, botClient) {
        _db = db;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task<string> Consume(string[] args, Message message, CancellationToken cancellationToken) {
        if (args.Length is 0) {
            return await GetDomainList(message, cancellationToken);
        }

        if (!args[0].TryGetDomainFromInput(out var domain)) {
            return "Domain format is not valid. Should be like google.com";
        }

        var response = await _mediator.CreateRequestClient<UpdateDomainMonitor>()
            .GetResponse<DomainMonitor, MessageResponse>(new {
                Domain = domain,
                ChatId = message.Chat.Id,
                Delete = args is [_, "remove", ..]
            }, cancellationToken);

        if (response.Is(out Response<MessageResponse>? error)) {
            return error.Message.Message;
        }

        if (!response.Is(out Response<DomainMonitor>? successResponse)) {
            _logger.LogError(new InvalidOperationException(), "Something went wrong");
        }

        var domainMonitor = successResponse!.Message;

        return $"Domain `{domain}` has been added to monitoring\n" +
               $"\n" +
               $"```\n" +
               $"Expired on:  {domainMonitor.ExpirationDate}\n" +
               $"Last update: {domainMonitor.LastUpdateDate}\n" +
               $"```";
    }

    private async Task<string> GetDomainList(Message message, CancellationToken cancellationToken) {
        var monitors = await _db.DomainMonitorByChat
            .Where(m => m.ChatId == message.Chat.Id)
            .Select(m => m.DomainMonitor)
            .Select(d => $"{d.Domain,-30} {d.ExpirationDate,17:g} {d.LastUpdateDate,17:g}")
            .ToListAsync(cancellationToken);

        return monitors.Count switch {
            0 => "Add your first domain to monitor by `/domain_monitor domain.com`.\n" +
                 "Or `/domain_monitor help` to get command help.",

            _ => "```\n" +
                 $"{"  Domain",-30} {"Expired on   ",17} {"Last update   ",17}\n" +
                 string.Join('\n', monitors) + "\n" +
                 "```"
        };
    }
}