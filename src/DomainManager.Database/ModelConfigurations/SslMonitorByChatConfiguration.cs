using DomainManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainManager.ModelConfigurations;

public class SslMonitorByChatConfiguration : IEntityTypeConfiguration<SslMonitorByChat> {
    public void Configure(EntityTypeBuilder<SslMonitorByChat> builder) {
        builder.HasKey(chat => new { chat.ChatId, chat.SslMonitorId });
    }
}