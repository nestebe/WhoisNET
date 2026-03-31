using System.Text.Json;
using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET;

/// <summary>
/// Main client for performing domain lookups.
/// Uses RDAP first with WHOIS fallback.
/// </summary>
public interface IDomainLookupClient : IDisposable
{
    /// <summary>
    /// Performs a complete lookup on a domain name.
    /// </summary>
    /// <param name="domain">Domain name (e.g., "example.com").</param>
    /// <param name="options">Lookup options (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Structured domain information.</returns>
    Task<DomainInfo> LookupAsync(
        string domain,
        LookupOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a domain is available for registration.
    /// </summary>
    /// <param name="domain">Domain name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the domain is available.</returns>
    Task<bool> IsAvailableAsync(
        string domain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the raw WHOIS response text.
    /// </summary>
    /// <param name="domain">Domain name to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Raw WHOIS text.</returns>
    Task<string> GetRawWhoisAsync(
        string domain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the raw RDAP JSON response.
    /// </summary>
    /// <param name="domain">Domain name to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Raw RDAP JSON document, or null if unavailable.</returns>
    Task<JsonDocument?> GetRawRdapAsync(
        string domain,
        CancellationToken cancellationToken = default);
}
