using DomainManager;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.ClearProviders();
    var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();
    loggingBuilder.AddSerilog(logger);
});
builder.AddBusiness();
builder.AddApplicationDbContext();

var host = builder.Build();

host.MigrateDatabase();

host.Run();