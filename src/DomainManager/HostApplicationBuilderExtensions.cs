using Serilog;

namespace DomainManager;

public static class HostApplicationBuilderExtensions {
    public static HostApplicationBuilder AddLogging(this HostApplicationBuilder builder) {
        builder.Services.AddLogging(loggingBuilder => {
            loggingBuilder.ClearProviders();
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
            loggingBuilder.AddSerilog(logger);
        });
        return builder;
    }
}