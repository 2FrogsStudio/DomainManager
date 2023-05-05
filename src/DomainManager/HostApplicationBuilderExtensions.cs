using Serilog;

namespace DomainManager;

public static class HostApplicationBuilderExtensions {
    public static HostApplicationBuilder AddLogging(this HostApplicationBuilder builder) {
        // fix sentry environment
        Environment.SetEnvironmentVariable("SENTRY_ENVIRONMENT", builder.Environment.EnvironmentName);

        builder.Services.AddLogging(loggingBuilder => {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger());
        });
        return builder;
    }
}