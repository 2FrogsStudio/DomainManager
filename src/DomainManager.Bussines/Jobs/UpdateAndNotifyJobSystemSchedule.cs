using MassTransit.Scheduling;

namespace DomainManager.Jobs;

public class UpdateAndNotifyJobSystemSchedule : DefaultRecurringSchedule {
    public static readonly UpdateAndNotifyJobSystemSchedule DefaultInstance = new(null!);

    public UpdateAndNotifyJobSystemSchedule(string cronExpression) {
        CronExpression = cronExpression;
    }
}