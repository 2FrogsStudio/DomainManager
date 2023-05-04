using DomainManager.Abstract;
using DomainManager.Configuration;
using DomainManager.Jobs;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DomainManager.Notifications.UpdateConsumers;

public class ScheduleUpdateAndNotifyJobConsumer : IConsumer<UpdateNotification>, IMediatorConsumer {
    private const string Help = "```\n" +
                                "schedule off - disable monitoring job\n" +
                                "schedule run - force run job\n" +
                                "schedule status - get job status\n" +
                                "schedule [cron] - enable monitoring job\n" +
                                "  cron e.g. '0 0 12 ? * 2-6 *' - fire monitoring job every 12 hours from monday to friday" +
                                "```";

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
            ["schedule", "off"] => await DisableSchedule(),
            ["schedule", "status"] => await GetSchedule(cancellationToken),
            ["schedule", "run"] => await RunSchedule(cancellationToken),
            ["schedule", "help"] => Help,
            ["schedule", var cron] => await EnableSchedule(cron, cancellationToken),
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

    private async Task<string> RunSchedule(CancellationToken cancellationToken) {
        var formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdateAndNotifyJobConsumer>();
        var endpoint = new Uri($"queue:{formatter}");
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(endpoint);
        await sendEndpoint.Send<UpdateAndNotifyJob>(new { }, cancellationToken);
        return "Job ahs been started";
    }

    private async Task<string> GetSchedule(CancellationToken cancellationToken) {
        var schedule = UpdateAndNotifyJobSystemSchedule.DefaultInstance;
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

    private static bool TryCheckCronExpression(string cron, out string error) {
        try {
            CronScheduleBuilder.CronSchedule(cron).Build();
        } catch (FormatException ex) {
            error = ex.Message;
            return false;
        }

        error = null!;
        return true;
    }

    private async Task<string> EnableSchedule(string cron, CancellationToken cancellationToken) {
        if (TryCheckCronExpression(cron, out var error)) {
            var sendEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
            var formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdateAndNotifyJobConsumer>();
            var endpoint = new Uri($"queue:{formatter}");
            var schedule = new UpdateAndNotifyJobSystemSchedule(cron);
            await sendEndpoint.ScheduleRecurringSend<UpdateAndNotifyJob>(endpoint, schedule, new { },
                cancellationToken);
            return $"Monitoring job has been enabled with cron: `{cron}`";
        }

        _logger.LogWarning("{ReplyMessage}", error);
        return $"Error: `{error}`";
    }

    private async Task<string> DisableSchedule() {
        var sendEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
        await sendEndpoint.CancelScheduledRecurringSend(UpdateAndNotifyJobSystemSchedule.DefaultInstance.ScheduleId,
            UpdateAndNotifyJobSystemSchedule.DefaultInstance.ScheduleGroup);
        return "Monitoring job has been disabled";
    }
}