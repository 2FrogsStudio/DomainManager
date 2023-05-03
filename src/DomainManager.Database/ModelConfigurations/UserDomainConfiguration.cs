using DomainManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainManager.ModelConfigurations;

public class UserDomainConfiguration : IEntityTypeConfiguration<UserDomains> {
    public void Configure(EntityTypeBuilder<UserDomains> builder) {
        builder.HasKey(userDomain => new { userDomain.Id, userDomain.Domain });
    }
}