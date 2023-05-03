using DomainManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainManager.ModelConfigurations;

public class SslExpireConfiguration : IEntityTypeConfiguration<SslExpire> {
    public void Configure(EntityTypeBuilder<SslExpire> builder) { }
}