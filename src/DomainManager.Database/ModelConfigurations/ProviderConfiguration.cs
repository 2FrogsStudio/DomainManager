using DomainManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainManager.ModelConfigurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider> {
    public void Configure(EntityTypeBuilder<Provider> builder) {
        builder.HasData
        (
            new Provider {
                Id = 1,
                Name = "Digital Ocean"
            },
            new Provider {
                Id = 2,
                Name = "Gandy"
            }
        );
    }
}