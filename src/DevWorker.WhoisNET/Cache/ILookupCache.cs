using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET.Cache;

/// <summary>
/// Cache for domain lookup results.
/// </summary>
public interface ILookupCache
{
    /// <summary>Gets a cached lookup result.</summary>
    Task<DomainInfo?> GetAsync(string domain, CancellationToken cancellationToken = default);

    /// <summary>Stores a lookup result in cache.</summary>
    Task SetAsync(string domain, DomainInfo info, TimeSpan? duration = null, CancellationToken cancellationToken = default);

    /// <summary>Removes a cached entry.</summary>
    Task RemoveAsync(string domain, CancellationToken cancellationToken = default);

    /// <summary>Clears all cached entries.</summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
