using DevWorker.WhoisNET.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DevWorker.WhoisNET.Cache;

/// <summary>
/// In-memory implementation of <see cref="ILookupCache"/> using <see cref="IMemoryCache"/>.
/// </summary>
internal sealed class MemoryLookupCache : ILookupCache
{
    private const string Prefix = "whoisnet:";
    private readonly IMemoryCache _cache;
    private readonly WhoisNetOptions _options;

    internal MemoryLookupCache(IMemoryCache cache, IOptions<WhoisNetOptions>? options = null)
    {
        _cache = cache;
        _options = options?.Value ?? new WhoisNetOptions();
    }

    /// <inheritdoc />
    public Task<DomainInfo?> GetAsync(string domain, CancellationToken cancellationToken = default)
    {
        var key = GetKey(domain);
        _cache.TryGetValue(key, out DomainInfo? result);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task SetAsync(string domain, DomainInfo info, TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        var key = GetKey(domain);
        _cache.Set(key, info, duration ?? _options.CacheDuration);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string domain, CancellationToken cancellationToken = default)
    {
        var key = GetKey(domain);
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        // IMemoryCache does not support clearing all entries.
        // Users needing this should use a custom implementation.
        return Task.CompletedTask;
    }

    private static string GetKey(string domain)
    {
        return Prefix + domain.Trim().ToLowerInvariant().TrimEnd('.');
    }
}
