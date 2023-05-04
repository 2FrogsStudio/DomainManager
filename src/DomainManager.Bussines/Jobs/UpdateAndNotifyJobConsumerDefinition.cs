using MassTransit;

namespace DomainManager.Jobs;

public class UpdateAndNotifyJobConsumerDefinition : ConsumerDefinition<UpdateAndNotifyJobConsumer> {
    public UpdateAndNotifyJobConsumerDefinition() {
        ConcurrentMessageLimit = 1;
    }
}