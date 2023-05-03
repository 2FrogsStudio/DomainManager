using DomainManager.Notifications;
using DomainManager.Notifications.CommandHandlers;
using DomainManager.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Whois;

namespace DomainManager;

public static class HostApplicationBuilderExtensions {
    public static HostApplicationBuilder AddBusiness(this HostApplicationBuilder builder) {
        builder.Services.AddHttpClient("TelegramBotClient")
            .AddTypedClient<ITelegramBotClient>((client, provider) => {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var botToken = configuration["Bot:Token"];
                if (botToken is null or "YOUR_ACCESS_TOKEN_HERE") {
                    throw new NullReferenceException("Bot:Token should be provided");
                }

                return new TelegramBotClient(botToken, client);
            });
        builder.Services.AddMassTransit(configurator => configurator.UsingInMemory())
            .AddMediator(cfg => {
                cfg.AddConsumers(typeof(UpdateNotification).Assembly);
                cfg.ConfigureMediator((context, mcfg) => {
                    mcfg.UseSendFilter(typeof(AnyCommandHelpFilter<>), context);
                });
            });
        builder.Services.AddHostedService<PullingService>()
            .AddTransient<IUpdateHandler, UpdateHandler>();
        builder.Services.AddSingleton<IStaticService, StaticService>();
        builder.Services.AddHostedService<BotInit>();
        builder.Services.AddSingleton<IWhoisLookup, WhoisLookup>();
        return builder;
    }
}