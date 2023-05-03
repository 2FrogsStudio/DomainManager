using Microsoft.Extensions.DependencyInjection;
using Whois;

namespace DomainManager;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddWhoisService(this IServiceCollection serviceCollection) {
        return serviceCollection.AddSingleton<IWhoisLookup, WhoisLookup>();
    }
}