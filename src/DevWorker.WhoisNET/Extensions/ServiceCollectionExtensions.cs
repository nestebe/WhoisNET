using DevWorker.WhoisNET.Cache;
using DevWorker.WhoisNET.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DevWorker.WhoisNET.Extensions;

/// <summary>
/// Extension methods for registering WhoisNET services in a DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers WhoisNET services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWhoisNet(
        this IServiceCollection services,
        Action<WhoisNetOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddMemoryCache();
        services.AddHttpClient<IRdapClient, RdapClient>();
        services.AddSingleton<IWhoisClient, WhoisClient>();
        services.AddSingleton<IDomainLookupClient, DomainLookupClient>();
        services.AddSingleton<ILookupCache, MemoryLookupCache>();

        return services;
    }
}
