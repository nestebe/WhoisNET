using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET;

/// <summary>
/// Client for WHOIS TCP (port 43) queries.
/// </summary>
public interface IWhoisClient
{
    /// <summary>
    /// Queries a WHOIS server for domain information.
    /// </summary>
    /// <param name="domain">Domain name to query.</param>
    /// <param name="whoisServer">WHOIS server to query (null = auto-resolve).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Raw WHOIS response.</returns>
    Task<WhoisRawResponse> QueryAsync(
        string domain,
        string? whoisServer = null,
        CancellationToken cancellationToken = default);
}
