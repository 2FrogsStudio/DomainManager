using Microsoft.Extensions.DependencyInjection;
using Whois;

namespace DomainManager;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddBusiness(this IServiceCollection services) {
        services.AddSingleton<IWhoisLookup, WhoisLookup>();
        return services;
    }
}