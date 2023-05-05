namespace DomainManager.Jobs;

// ReSharper disable once InconsistentNaming
public interface UpdateAndNotifyJob {
    public long ChatId { get; set; }
}