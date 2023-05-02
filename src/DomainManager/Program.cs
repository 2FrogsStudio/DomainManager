using DomainManager.Notifications;
using DomainManager.Services;
using MassTransit;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => {
        services.AddHttpClient("TelegramBotClient")
            .AddTypedClient<ITelegramBotClient>((client, provider) => {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var botToken = configuration["Bot:Token"];
                if (botToken is null or "YOUR_ACCESS_TOKEN_HERE") {
                    throw new NullReferenceException("Bot:Token should be provided");
                }

                return new TelegramBotClient(botToken, client);
            });
        services.AddMassTransit(configurator => configurator.UsingInMemory())
            .AddMediator(cfg => { cfg.AddConsumers(typeof(UpdateNotification).Assembly); });
        services.AddHostedService<PullingService>()
            .AddTransient<IUpdateHandler, UpdateHandler>();
        services.AddSingleton<IStaticService, StaticService>();
    })
    .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext())
    .Build();

host.Run();