using System.Reflection;
using MassTransit;
using MassTransit.Scheduling;

namespace DomainManager.Jobs;

public class UpdateAndNotifyJobSystemSchedule : RecurringSchedule {
    public UpdateAndNotifyJobSystemSchedule(long chatId) {
        MisfirePolicy = MissedEventPolicy.Send;
        ScheduleId = $"{TypeCache.GetShortName(GetType())}_{chatId}";
        ScheduleGroup =
            GetType().GetTypeInfo().Assembly.FullName!
                .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];

        TimeZoneId = TimeZoneInfo.Local.Id;
        StartTime = DateTime.Now;
    }

    public UpdateAndNotifyJobSystemSchedule(string cronExpression, long chatId) {
        CronExpression = cronExpression;
        MisfirePolicy = MissedEventPolicy.Send;
        ScheduleId = $"{TypeCache.GetShortName(GetType())}_{chatId}";
        ScheduleGroup =
            GetType().GetTypeInfo().Assembly.FullName!.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                [0];

        TimeZoneId = TimeZoneInfo.Local.Id;
        StartTime = DateTime.Now;
    }

    public MissedEventPolicy MisfirePolicy { get; protected set; }
    public string TimeZoneId { get; protected set; }
    public DateTimeOffset StartTime { get; protected set; }
    public DateTimeOffset? EndTime { get; protected set; } = null!;
    public string ScheduleId { get; }
    public string ScheduleGroup { get; }
    public string CronExpression { get; protected set; } = null!;
    public string Description { get; protected set; } = null!;
}