using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace DomainManager;

public static class HostApplicationBuilderExtensions {
    public static HostApplicationBuilder AddLogging(this HostApplicationBuilder builder) {
        builder.Services.AddLogging(loggingBuilder => {
            loggingBuilder.AddConfiguration(builder.Configuration);

            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger());

            loggingBuilder.AddSentry(options => { options.Environment = builder.Environment.EnvironmentName; });
        });
        return builder;
    }

    public static HostApplicationBuilder AddMemoryCache(this HostApplicationBuilder builder) {
        builder.Services.AddMemoryCache();
        builder.Services.Configure<MemoryCacheOptions>(builder.Configuration.GetSection("MemoryCache"));

        return builder;
    }
}