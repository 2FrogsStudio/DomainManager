using System.Text;
using DomainManager.Models;
using DomainManager.Requests;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandHandlers;

public class DomainMonitorCommandHandler : CommandHandlerBase {
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

        return new StringBuilder()
            .AppendLine($"Domain `{domain}` has been added to monitoring")
            .AppendLine()
            .AppendLine("```")
            .AppendLine($"{"Expires On:",-12} {domainMonitor.ExpirationDate}")
            .AppendLine($"{"Last update:",-12} {domainMonitor.LastUpdateDate}")
            .AppendLine("```")
            .ToString();
    }

    private async Task<string> GetDomainList(Message message, CancellationToken cancellationToken) {
        var monitors = await _db.DomainMonitorByChat
            .Where(m => m.ChatId == message.Chat.Id)
            .Select(m => m.DomainMonitor)
            .Select(d => $"{d.Domain,-30} {d.ExpirationDate,17:g} {d.LastUpdateDate,17:g}")
            .ToListAsync(cancellationToken);

        return monitors.Count == 0
            ? "Add your first domain by `/domain_monitor [domain]`"
            : new StringBuilder()
                .AppendLine("```")
                .AppendLine($"{"  Domain",-30} {"Expired on   ",17} {"Last update   ",17}")
                .AppendLine(string.Join('\n', monitors))
                .AppendLine("```")
                .ToString();
    }
}