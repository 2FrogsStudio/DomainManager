using DomainManager.Abstract;
using DomainManager.Configuration;
using DomainManager.Jobs;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Spi;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DomainManager.Notifications.UpdateConsumers;

public class ScheduleUpdateAndNotifyJobConsumer : IConsumer<UpdateNotification>, IMediatorConsumer {
    private const string Help = "```\n" +
                                "schedule [cron_expr] - enable monitoring job\n" +
                                "schedule off         - disable monitoring job\n" +
                                "schedule run         - force run updating job\n" +
                                "schedule status      - get job status\n" +
                                "  cron_expr e.g. '0 0 12 ? * 2-6 *' - fire monitoring job every 12 hours from monday to friday" +
                                "```";

    private static readonly TimeSpan MinimumScheduleTime = TimeSpan.FromHours(1);

    private readonly ITelegramBotClient _botClient;
    private readonly IOptions<BotOptions> _botOptions;
    private readonly IBus _bus;
    private readonly ILogger<ScheduleUpdateAndNotifyJobConsumer> _logger;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public ScheduleUpdateAndNotifyJobConsumer(ILogger<ScheduleUpdateAndNotifyJobConsumer> logger,
        ITelegramBotClient botClient, IOptions<BotOptions> botOptions, IBus bus,
        ISchedulerFactory schedulerFactory, Bind<IBus, ISendEndpointProvider> sendEndpointProvider) {
        _logger = logger;
        _botClient = botClient;
        _botOptions = botOptions;
        _bus = bus;
        _schedulerFactory = schedulerFactory;
        _sendEndpointProvider = sendEndpointProvider.Value;
    }

    public async Task Consume(ConsumeContext<UpdateNotification> context) {
        var update = context.Message.Update;
        var cancellationToken = context.CancellationToken;

        if (update is not {
                Message : {
                    Text: { } messageText,
                    MessageId: var messageId,
                    Chat.Id: var chatId,
                    From.Id: var fromId
                }
            }
            || !_botOptions.Value.AdminUserIds.Contains(fromId)
            // can be private chat with admin
            || !(_botOptions.Value.AdminUserIds.Contains(chatId) || _botOptions.Value.AdminGroupIds.Contains(chatId))
            || !messageText.StartsWith("schedule")) {
            return;
        }

        var replyMessage = messageText.Split(' ', 2) switch {
            ["schedule", "off"] => await DisableSchedule(chatId),
            ["schedule", "status"] => await GetSchedule(chatId, cancellationToken),
            ["schedule", "run"] => await RunSchedule(chatId, cancellationToken),
            ["schedule", "help"] => Help,
            ["schedule", var cron] => await EnableSchedule(chatId, cron, cancellationToken),
            _ => Help
        };

        await _botClient.SendTextMessageAsync(
            chatId,
            replyMessage,
            ParseMode.Markdown,
            replyToMessageId: messageId,
            cancellationToken: context.CancellationToken
        );
    }

    private async Task<string> RunSchedule(long chatId, CancellationToken cancellationToken) {
        var formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdateAndNotifyJobConsumer>();
        var endpoint = new Uri($"queue:{formatter}");
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(endpoint);
        await sendEndpoint.Send<UpdateAndNotifyJob>(new {
            ChatId = chatId
        }, cancellationToken);
        return "Job has been started";
    }

    private async Task<string> GetSchedule(long chatId, CancellationToken cancellationToken) {
        var schedule = new UpdateAndNotifyJobSystemSchedule(chatId);
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var triggerKey = new TriggerKey(QuartzConstants.RecurringTriggerPrefix + schedule.ScheduleId,
            schedule.ScheduleGroup);
        var trigger = await scheduler.GetTrigger(triggerKey, cancellationToken);
        var state = await scheduler.GetTriggerState(triggerKey, cancellationToken);

        string? cronExp = null;
        string? nextFire = null;
        if (trigger != null) {
            nextFire = $"Next fire: {trigger.GetNextFireTimeUtc():g}\n";
            cronExp = trigger is ICronTrigger cronTrigger ? $"Cron: {cronTrigger.CronExpressionString}\n" : null;
        }

        return $"```\nStatus: {state}\n" +
               $"{nextFire}" +
               $"{cronExp}```";
    }

    private bool TryCheckCronExpression(string cron, out string error) {
        IMutableTrigger trigger;
        try {
            // CronExpression.ValidateExpression(cron);
            // Build trigger instead to calculate interval
            trigger = CronScheduleBuilder.CronSchedule(cron).Build();
        } catch (FormatException ex) {
            error = ex.Message;
            return false;
        }

        var next1 = trigger.GetFireTimeAfter(DateTime.MinValue.ToUniversalTime()) ??
                    DateTimeOffset.MinValue.UtcDateTime;
        var next2 = trigger.GetFireTimeAfter(next1) ?? DateTimeOffset.MaxValue.UtcDateTime;
        var diff = next2 - next1;
        _logger.LogInformation("{Next1} - {Next2}", next1, next2);
        if (next2 - next1 < MinimumScheduleTime) {
            error =
                $"Schedule cannot be less than {MinimumScheduleTime.TotalHours:G} hours, now is {diff.ToHumanReadableString()}";
            return false;
        }

        error = null!;
        return true;
    }

    private async Task<string> EnableSchedule(long chatId, string cron, CancellationToken cancellationToken) {
        if (TryCheckCronExpression(cron, out var error)) {
            var sendEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
            var formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdateAndNotifyJobConsumer>();
            var endpoint = new Uri($"queue:{formatter}");
            var schedule = new UpdateAndNotifyJobSystemSchedule(cron, chatId);
            await sendEndpoint.ScheduleRecurringSend<UpdateAndNotifyJob>(endpoint, schedule, new {
                    ChatId = chatId
                },
                cancellationToken);
            return $"Monitoring job has been enabled with cron: `{cron}`";
        }

        _logger.LogWarning("{ReplyMessage}", error);
        return $"Error: `{error}`";
    }

    private async Task<string> DisableSchedule(long chatId) {
        var schedule = new UpdateAndNotifyJobSystemSchedule(chatId);
        var sendEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
        await sendEndpoint.CancelScheduledRecurringSend(schedule.ScheduleId, schedule.ScheduleGroup);
        return "Monitoring job has been disabled";
    }
}