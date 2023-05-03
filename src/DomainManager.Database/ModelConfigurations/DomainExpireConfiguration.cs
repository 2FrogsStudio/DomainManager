using DomainManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainManager.ModelConfigurations;

public class DomainExpireConfiguration : IEntityTypeConfiguration<DomainExpire> {
    public void Configure(EntityTypeBuilder<DomainExpire> builder) {
        builder.HasKey(userDomain => new { Id = userDomain.ChatId, userDomain.Domain });
    }
}