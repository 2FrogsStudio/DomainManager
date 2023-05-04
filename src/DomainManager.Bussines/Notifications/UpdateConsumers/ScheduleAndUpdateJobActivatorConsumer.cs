using DomainManager.Abstract;
using DomainManager.Configuration;
using DomainManager.Jobs;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DomainManager.Notifications.UpdateConsumers;

public class ScheduleAndUpdateJobActivatorConsumer : IConsumer<UpdateNotification>, IMediatorConsumer {
    private readonly ITelegramBotClient _botClient;
    private readonly IOptions<BotOptions> _botOptions;
    private readonly ISecondBus _bus;
    private readonly ILogger<ScheduleAndUpdateJobActivatorConsumer> _logger;

    public ScheduleAndUpdateJobActivatorConsumer(ILogger<ScheduleAndUpdateJobActivatorConsumer> logger,
        ITelegramBotClient botClient, IOptions<BotOptions> botOptions, ISecondBus bus) {
        _logger = logger;
        _botClient = botClient;
        _botOptions = botOptions;
        _bus = bus;
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

        string replyMessage;
        switch (messageText.Split(' ', 2)) {
            case ["schedule", "off"]:
                await DisableSchedule(cancellationToken);
                replyMessage = "Monitoring job has been disabled";
                break;
            case ["schedule", var cron]:
                if (TryCheckCronExpression(cron, out var error)) {
                    await EnableSchedule(cron, cancellationToken);
                    replyMessage = $"Monitoring job has been enabled with cron: `{cron}`";
                } else {
                    replyMessage = $"Error: `{error}`";
                    _logger.LogWarning("{ReplyMessage}", replyMessage);
                }

                break;
            default:
                replyMessage = "```\n" +
                               "schedule off - disable monitoring job\n" +
                               "schedule [cron] - enable monitoring job\n" +
                               "  cron e.g. '0 0 */12 * * MON-FRI' - fire monitoring job every 12 hours from monday to friday" +
                               "```";
                break;
        }

        await _botClient.SendTextMessageAsync(
            chatId,
            replyMessage,
            ParseMode.Markdown,
            replyToMessageId: messageId,
            cancellationToken: context.CancellationToken
        );
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

    private async Task EnableSchedule(string cron, CancellationToken cancellationToken) {
        var sendEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
        var formatter = DefaultEndpointNameFormatter.Instance.Consumer<UpdateAndNotifyJobConsumer>();
        var endpoint = new Uri($"queue:{formatter}");
        var schedule = new UpdateAndNotifyJobSystemSchedule(cron);
        await sendEndpoint.ScheduleRecurringSend<UpdateAndNotifyJob>(endpoint, schedule, new { }, cancellationToken);
    }

    private async Task DisableSchedule(CancellationToken cancellationToken) {
        var sendEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
        var schedule = new UpdateAndNotifyJobSystemSchedule("0 0 0 0 0 0");
        await sendEndpoint.CancelScheduledRecurringSend(schedule.ScheduleId, schedule.ScheduleGroup);
    }
}