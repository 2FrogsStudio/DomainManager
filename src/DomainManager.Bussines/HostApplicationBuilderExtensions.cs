using DomainManager.Abstract;
using DomainManager.Configuration;
using DomainManager.Jobs;
using DomainManager.Notifications.UpdateConsumers;
using DomainManager.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Whois;

namespace DomainManager;

public static class HostApplicationBuilderExtensions {
    public static HostApplicationBuilder AddBusiness(this HostApplicationBuilder builder) {
        builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Bot"));

        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient("TelegramBotClient")
            .AddTypedClient<ITelegramBotClient>((client, provider) => {
                var botToken = provider.GetRequiredService<IOptions<BotOptions>>().Value.Token;
                if (botToken is null or "YOUR_ACCESS_TOKEN_HERE") {
                    throw new NullReferenceException("Bot:Token should be provided");
                }

                return new TelegramBotClient(botToken, client);
            });
        builder.Services.AddQuartz(q => {
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.UsePersistentStore(s => {
                s.UsePostgres(server =>
                    server.ConnectionString = builder.Configuration.GetConnectionString("Supabase")!);
                s.UseJsonSerializer();
                s.UseClustering();
            });
        });
        builder.Services
            .AddMediator(cfg => {
                cfg.AddConsumers(type => type.IsAssignableTo(typeof(IMediatorConsumer)),
                    typeof(SendCommandNotificationConsumer).Assembly);
            })
            .AddMassTransit<IBus>(x => {
                x.AddQuartzConsumers();
                x.AddConsumers(type => !type.IsAssignableTo(typeof(IMediatorConsumer)),
                    typeof(UpdateAndNotifyJobConsumer).Assembly);
                x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });
            });
        builder.Services.AddHostedService<PullingService>()
            .AddTransient<IUpdateHandler, UpdateHandler>();
        builder.Services.AddSingleton<IStaticService, StaticService>();
        builder.Services.AddSingleton<IWhoisLookup, WhoisLookup>();
        builder.Services.AddHostedService<BotInit>();
        return builder;
    }
}