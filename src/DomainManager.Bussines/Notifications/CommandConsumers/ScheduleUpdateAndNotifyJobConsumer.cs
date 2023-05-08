using DomainManager.Abstract;
using DomainManager.Jobs;
using DomainManager.Notifications.CommandConsumers.Base;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Quartz;
using Quartz.Spi;
using Telegram.Bot;

namespace DomainManager.Notifications.CommandConsumers;

public class ScheduleUpdateAndNotifyJobConsumer : CommandConsumerBase, IMediatorConsumer {
    private static readonly TimeSpan MinimumScheduleTime = TimeSpan.FromHours(1);

    private readonly ILogger<ScheduleUpdateAndNotifyJobConsumer> _logger;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public ScheduleUpdateAndNotifyJobConsumer(ILogger<ScheduleUpdateAndNotifyJobConsumer> logger,
        ITelegramBotClient botClient, ISchedulerFactory schedulerFactory,
        Bind<IBus, ISendEndpointProvider> sendEndpointProvider, IMemoryCache memoryCache) : base(
        Command.Schedule, botClient, memoryCache) {
        _logger = logger;
        _schedulerFactory = schedulerFactory;
        _sendEndpointProvider = sendEndpointProvider.Value;
    }

    protected override async Task<string> Consume(string[] args, Message message, long chatId, bool isAdmin,
        CancellationToken cancellationToken) {
        return args switch {
            ["status", ..] => await GetSchedule(chatId, cancellationToken),
            ["off", ..] when isAdmin => await DisableSchedule(chatId),
            ["off", ..] => "You have no access to control job",
            ["run", ..] when isAdmin => await RunSchedule(chatId, cancellationToken),
            ["run", ..] => "You have no access to control job",
            _ when isAdmin => await EnableSchedule(chatId, string.Join(' ', args), cancellationToken),
            _ => "You have no access to control job"
        };
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
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:quartz"));
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
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:quartz"));
        await sendEndpoint.CancelScheduledRecurringSend(schedule.ScheduleId, schedule.ScheduleGroup);
        return "Monitoring job has been disabled";
    }
}