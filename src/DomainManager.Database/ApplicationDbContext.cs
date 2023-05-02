using DomainManager.ModelConfigurations;
using DomainManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainManager;

public class ApplicationDbContext : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Provider> Providers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
    }
}