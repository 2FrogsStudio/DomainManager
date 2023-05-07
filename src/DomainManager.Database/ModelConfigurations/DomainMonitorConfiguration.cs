namespace DomainManager.ModelConfigurations;

public class DomainMonitorConfiguration : IEntityTypeConfiguration<DomainMonitor> {
    public void Configure(EntityTypeBuilder<DomainMonitor> builder) {
        builder.Property(c => c.Domain)
            .UseCollation("case_insensitive");
    }
}