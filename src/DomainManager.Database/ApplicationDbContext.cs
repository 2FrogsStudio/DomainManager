namespace DomainManager;

public class ApplicationDbContext : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<DomainMonitor> DomainMonitor { get; set; } = null!;
    public DbSet<SslMonitor> SslMonitor { get; set; } = null!;

    public DbSet<DomainMonitorByChat> DomainMonitorByChat { get; set; } = null!;
    public DbSet<SslMonitorByChat> SslMonitorByChat { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.HasCollation("case_insensitive", "en-u-ks-primary", "icu", false);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}