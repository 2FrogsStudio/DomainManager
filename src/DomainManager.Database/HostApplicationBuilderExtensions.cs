using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DomainManager;

public static class HostApplicationBuilderExtensions {
    public static HostApplicationBuilder AddApplicationDbContext(this HostApplicationBuilder builder) {
        builder.Services
            .AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Supabase")));
        return builder;
    }
}