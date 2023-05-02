using DomainManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainManager.ModelConfigurations;

public class UserTokensConfiguration : IEntityTypeConfiguration<UserTokens> {
    public void Configure(EntityTypeBuilder<UserTokens> builder) {
        builder.HasKey(userTokens => new { userTokens.Id, userTokens.ProviderId });
    }
}