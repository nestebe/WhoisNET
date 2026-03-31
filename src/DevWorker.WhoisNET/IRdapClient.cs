using System.Text.Json;
using DevWorker.WhoisNET.Models;
using DevWorker.WhoisNET.Rdap;

namespace DevWorker.WhoisNET;

/// <summary>
/// Client for RDAP (RFC 7480-7484) HTTP queries.
/// </summary>
public interface IRdapClient
{
    /// <summary>
    /// Performs an RDAP lookup and returns structured domain info.
    /// </summary>
    /// <param name="domain">Domain name to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Structured domain info, or null if RDAP is unavailable for this TLD.</returns>
    Task<DomainInfo?> LookupAsync(
        string domain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the raw RDAP JSON response.
    /// </summary>
    /// <param name="domain">Domain name to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Raw RDAP response, or null if RDAP is unavailable.</returns>
    Task<RdapResponse?> GetRawResponseAsync(
        string domain,
        CancellationToken cancellationToken = default);
}
