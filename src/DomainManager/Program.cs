using DomainManager;
using DomainManager.Notifications;
using DomainManager.Services;
using MassTransit;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.ClearProviders();
    var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();
    loggingBuilder.AddSerilog(logger);
});
builder.Services.AddHttpClient("TelegramBotClient")
    .AddTypedClient<ITelegramBotClient>((client, provider) => {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var botToken = configuration["Bot:Token"];
        if (botToken is null or "YOUR_ACCESS_TOKEN_HERE") {
            throw new NullReferenceException("Bot:Token should be provided");
        }

        return new TelegramBotClient(botToken, client);
    });
builder.Services.AddHttpClient("CertificateExpiration")
    .ConfigurePrimaryHttpMessageHandler<CertificateToResponseHandler>();
builder.Services.AddSingleton<CertificateToResponseHandler>();
builder.Services.AddMassTransit(configurator => configurator.UsingInMemory())
    .AddMediator(cfg => cfg.AddConsumers(typeof(UpdateNotification).Assembly));
builder.Services.AddHostedService<PullingService>()
    .AddTransient<IUpdateHandler, UpdateHandler>();
builder.Services.AddSingleton<IStaticService, StaticService>();
builder.Services.AddHostedService<BotInit>();
builder.Services.AddBusiness();
builder.AddApplicationDbContext();

var host = builder.Build();

host.MigrateDatabase();

host.Run();