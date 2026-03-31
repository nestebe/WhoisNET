using System.Text;
using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DevWorker.WhoisNET.Whois;

/// <summary>
/// Resolves TLDs to their WHOIS server addresses.
/// </summary>
internal sealed class WhoisServerResolver
{
    private readonly ILogger _logger;
    private readonly WhoisNetOptions _options;
    private readonly WhoisTcpClient _tcpClient;

    internal WhoisServerResolver(
        WhoisTcpClient tcpClient,
        IOptions<WhoisNetOptions>? options = null,
        ILogger? logger = null)
    {
        _tcpClient = tcpClient;
        _options = options?.Value ?? new WhoisNetOptions();
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Gets the WHOIS server for a given TLD.
    /// First checks custom servers, then the built-in database, then falls back to IANA.
    /// </summary>
    internal async Task<string> GetServerAsync(string tld, CancellationToken cancellationToken = default)
    {
        tld = tld.ToLowerInvariant().TrimStart('.');

        // Check custom servers first
        if (_options.CustomServers.TryGetValue(tld, out var customServer))
        {
            _logger.LogDebug("Using custom WHOIS server {Server} for TLD {Tld}", customServer, tld);
            return customServer;
        }

        // Check built-in database
        if (WhoisServerDb.Servers.TryGetValue(tld, out var server))
        {
            _logger.LogDebug("Using built-in WHOIS server {Server} for TLD {Tld}", server, tld);
            return server;
        }

        // Fallback to IANA
        _logger.LogDebug("TLD {Tld} not in database, querying IANA", tld);
        return await QueryIanaAsync(tld, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the WHOIS server for a domain by extracting its TLD.
    /// </summary>
    internal async Task<string> GetServerForDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        var tld = TldHelper.GetTld(domain);
        return await GetServerAsync(tld, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> QueryIanaAsync(string tld, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _tcpClient.QueryAsync(
                "whois.iana.org",
                $"{tld}\r\n",
                Encoding.UTF8,
                TimeSpan.FromSeconds(10),
                cancellationToken).ConfigureAwait(false);

            foreach (var line in response.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("refer:", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("whois:", StringComparison.OrdinalIgnoreCase))
                {
                    var server = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(server))
                    {
                        _logger.LogDebug("IANA resolved TLD {Tld} to server {Server}", tld, server);
                        return server;
                    }
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to query IANA for TLD {Tld}", tld);
        }

        throw new WhoisServerNotFoundException(tld);
    }
}
