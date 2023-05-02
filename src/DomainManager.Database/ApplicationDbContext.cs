using DomainManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainManager;

public class ApplicationDbContext : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Provider> Providers { get; set; } = null!;
}