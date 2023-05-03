using DomainManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainManager.ModelConfigurations;

public class SslMonitorConfiguration : IEntityTypeConfiguration<SslMonitor> {
    public void Configure(EntityTypeBuilder<SslMonitor> builder) {
        builder.Property(c => c.Host)
            .UseCollation("case_insensitive");
    }
}