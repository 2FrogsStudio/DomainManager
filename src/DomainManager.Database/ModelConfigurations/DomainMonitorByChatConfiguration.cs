using DomainManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainManager.ModelConfigurations;

public class DomainMonitorByChatConfiguration : IEntityTypeConfiguration<DomainMonitorByChat> {
    public void Configure(EntityTypeBuilder<DomainMonitorByChat> builder) {
        builder.HasKey(chat => new { chat.ChatId, chat.DomainMonitorId });
    }
}