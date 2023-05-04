using System.Diagnostics;
using DomainManager.Requests;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Whois;

namespace DomainManager.Jobs;

public class UpdateAndNotifyJobConsumer : IConsumer<UpdateAndNotifyJob> {
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _db;
    private readonly TimeSpan _expirationTimeSpan = TimeSpan.FromDays(10);
    private readonly ILogger<UpdateAndNotifyJobConsumer> _logger;
    private readonly IScopedMediator _mediator;
    private readonly IWhoisLookup _whoisLookup;

    public UpdateAndNotifyJobConsumer(ILogger<UpdateAndNotifyJobConsumer> logger, IScopedMediator mediator,
        ApplicationDbContext db, IWhoisLookup whoisLookup, ITelegramBotClient botClient) {
        _logger = logger;
        _mediator = mediator;
        _db = db;
        _whoisLookup = whoisLookup;
        _botClient = botClient;
    }

    public async Task Consume(ConsumeContext<UpdateAndNotifyJob> context) {
        _logger.LogDebug("Start updating..");
        var cancellationToken = context.CancellationToken;
        await UpdateDomainMonitors(cancellationToken);
        await UpdateSslMonitors(cancellationToken);

        await NotifyDomainMonitors(cancellationToken);
        await NotifySslMonitors(cancellationToken);
        _logger.LogDebug("Updating finished");
    }

    private async Task NotifySslMonitors(CancellationToken cancellationToken) {
        var almostExpiredMessages = (await _db.SslMonitor
                .Where(monitor => monitor.NotAfter - DateTime.UtcNow <= _expirationTimeSpan)
                .Select(monitor => monitor.SslMonitors)
                .SelectMany(dm => dm)
                .Select(dm => new
                    { dm.ChatId, dm.SslMonitor.Host, dm.SslMonitor.NotAfter, dm.SslMonitor.LastUpdateDate })
                .ToArrayAsync(cancellationToken))
            .GroupBy(dm => dm.ChatId, arg => arg,
                (chatId, expiration) => new { chatId, expiration }
            );

        foreach (var ax in almostExpiredMessages) {
            var text = "Almost expired SSL certificate list:\n" +
                       "```\n" +
                       $"{"  Host",-30} {"Expired on   ",17} {"Last update   ",17}\n" +
                       string.Join('\n',
                           ax.expiration.Select(d => $"{d.Host,-30} {d.NotAfter,17:g} {d.LastUpdateDate,17:g}")) +
                       "```";
            await SendNotification(ax.chatId, text, cancellationToken);
        }
    }

    private async Task NotifyDomainMonitors(CancellationToken cancellationToken) {
        var almostExpiredMessages = (await _db.DomainMonitor
                .Where(monitor => monitor.ExpirationDate - DateTime.UtcNow <= _expirationTimeSpan)
                .Select(monitor => monitor.DomainMonitors)
                .SelectMany(dm => dm)
                .Select(dm => new {
                    dm.ChatId, dm.DomainMonitor.Domain, dm.DomainMonitor.ExpirationDate, dm.DomainMonitor.LastUpdateDate
                })
                .ToArrayAsync(cancellationToken))
            .GroupBy(dm => dm.ChatId, arg => arg,
                (chatId, expiration) => new { chatId, expiration }
            );

        foreach (var ax in almostExpiredMessages) {
            var text = "Almost expired domain list:\n" +
                       "```\n" +
                       $"{"  Domain",-30} {"Expired on   ",17} {"Last update   ",17}\n" +
                       string.Join('\n',
                           ax.expiration.Select(d =>
                               $"{d.Domain,-30} {d.ExpirationDate,17:g} {d.LastUpdateDate,17:g}")) +
                       "```";
            await SendNotification(ax.chatId, text, cancellationToken);
        }
    }

    private async Task SendNotification(long chatId, string text, CancellationToken cancellationToken) {
        try {
            await _botClient.SendTextMessageAsync(
                chatId,
                text,
                ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        } catch (Exception e) {
            _logger.LogError(e, "Something went wrong with notification");
        }
    }

    private async Task UpdateDomainMonitors(CancellationToken cancellationToken) {
        foreach (var monitor in await _db.DomainMonitor.ToArrayAsync(cancellationToken)) {
            _logger.LogDebug("Updating Domain monitor: {Domain}", monitor.Domain);
            try {
                var whois = await _whoisLookup.LookupAsync(monitor.Domain);
                monitor.LastUpdateDate = DateTime.UtcNow;
                monitor.ExpirationDate = whois.Expiration?.ToUniversalTime();
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to update domain monitor {Domain}", monitor.Domain);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateSslMonitors(CancellationToken cancellationToken) {
        foreach (var monitor in await _db.SslMonitor.ToArrayAsync(cancellationToken)) {
            _logger.LogDebug("Updating SSL monitor: {Domain}", monitor.Host);

            var response = await _mediator
                .CreateRequestClient<GetCertificateInfo>()
                .GetResponse<CertificateInfo, MessageResponse>(new { Hostname = monitor.Host }, cancellationToken);

            if (response.Is(out Response<CertificateInfo>? certInfoResponse)) {
                var certInfo = certInfoResponse.Message;
                monitor.LastUpdateDate = DateTime.UtcNow;
                monitor.Issuer = certInfo.Issuer;
                monitor.NotAfter = certInfo.NotAfter.ToUniversalTime();
                monitor.NotBefore = certInfo.NotBefore.ToUniversalTime();
                monitor.Errors = certInfo.Errors;
                continue;
            }

            if (response.Is(out Response<MessageResponse>? error)) {
                _logger.LogWarning("Failed to update ssl certificate info by host {Host}\n{Error}", monitor.Host,
                    error);
                continue;
            }

            throw new UnreachableException();
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}