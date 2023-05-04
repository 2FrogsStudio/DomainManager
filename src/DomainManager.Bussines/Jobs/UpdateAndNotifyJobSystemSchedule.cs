using MassTransit.Scheduling;

namespace DomainManager.Jobs;

public class UpdateAndNotifyJobSystemSchedule : DefaultRecurringSchedule {
    public UpdateAndNotifyJobSystemSchedule(string cronExpression) {
        CronExpression = cronExpression;
    }
}