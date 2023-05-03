using DomainManager.Models;
using DomainManager.Requests;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandHandlers;

public class SslMonitorCommandHandler : CommandHandlerBase {
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SslMonitorCommandHandler> _logger;
    private readonly IScopedMediator _mediator;

    public SslMonitorCommandHandler(ITelegramBotClient botClient, ApplicationDbContext db, IScopedMediator mediator,
        ILogger<SslMonitorCommandHandler> logger) :
        base(Command.SslMonitor, botClient) {
        _botClient = botClient;
        _db = db;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task<string> Consume(string[] args, Message message, CancellationToken cancellationToken) {
        if (args.Length is 0) {
            return await GetHostList(message, cancellationToken);
        }

        if (!args[0].TryGetDomainFromInput(out var host)) {
            return "Host format is not valid. Should be like google.com";
        }

        var response = await _mediator.CreateRequestClient<UpdateSslMonitor>()
            .GetResponse<SslMonitor, MessageResponse>(new {
                    ChatId = message.Chat.Id,
                    Host = host,
                    Delete = args is [_, "remove", ..]
                },
                cancellationToken);
        if (response.Is(out Response<MessageResponse>? error)) {
            return error.Message.Message;
        }

        if (!response.Is(out Response<SslMonitor>? sslMonitorResponse)) {
            _logger.LogError(new InvalidOperationException(), "Something went wrong");
        }

        var sslMonitor = sslMonitorResponse!.Message;
        return $"Host `{host}` has been added to monitoring\n" +
               $"\n" +
               $"```\n" +
               $"Expired on:  {sslMonitor.NotBefore}\n" +
               $"Last update: {sslMonitor.LastUpdateDate}\n" +
               $"```";
    }

    private async Task<string> GetHostList(Message message, CancellationToken cancellationToken) {
        var monitors = await _db.SslMonitorByChat
            .Where(m => m.ChatId == message.Chat.Id)
            .Select(m => m.SslMonitor)
            .Select(d => $"{d.Host,-30} {d.NotAfter,17:g} {d.LastUpdateDate,17:g}")
            .ToListAsync(cancellationToken);

        return monitors.Count switch {
            0 => "Add your first host by `/ssl_monitor my.site.com`\n" +
                 "Or `/ssl_monitor help` to get command help",

            _ => "```\n" +
                 $"{"  Host",-30} {"Expired on   ",17} {"Last update   ",17}\n" +
                 string.Join('\n', monitors) + "\n" +
                 "```"
        };
    }
}