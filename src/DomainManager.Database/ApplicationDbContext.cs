using DomainManager.ModelConfigurations;
using DomainManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainManager;

public class ApplicationDbContext : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<DomainMonitor> DnsMonitor { get; set; } = null!;
    public DbSet<SslMonitor> SslMonitor { get; set; } = null!;

    public DbSet<DomainMonitorByChat> DomainMonitorByChat { get; set; } = null!;
    public DbSet<SslMonitorByChat> SslMonitorByChat { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.HasCollation("case_insensitive", "en-u-ks-primary", "icu", false);
        modelBuilder.ApplyConfiguration(new DomainMonitorConfiguration());
        modelBuilder.ApplyConfiguration(new SslMonitorConfiguration());
        modelBuilder.ApplyConfiguration(new DomainMonitorByChatConfiguration());
        modelBuilder.ApplyConfiguration(new SslMonitorByChatConfiguration());
    }
}