using System.Text.Json;
using DevWorker.WhoisNET.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DevWorker.WhoisNET.Rdap;

/// <summary>
/// Resolves TLDs to their RDAP server URLs via the IANA bootstrap.
/// </summary>
internal sealed class RdapBootstrap
{
    private const string CacheKey = "whoisnet:rdap:bootstrap";

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly WhoisNetOptions _options;
    private readonly ILogger _logger;

    internal RdapBootstrap(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<WhoisNetOptions>? options = null,
        ILogger? logger = null)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options?.Value ?? new WhoisNetOptions();
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Gets the RDAP server URL for a TLD.
    /// </summary>
    /// <param name="tld">TLD (e.g., "com", "fr").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>RDAP server base URL, or null if not available.</returns>
    internal async Task<string?> GetServerUrlAsync(string tld, CancellationToken cancellationToken = default)
    {
        tld = tld.ToLowerInvariant().TrimStart('.');

        var mapping = await GetOrLoadBootstrapAsync(cancellationToken).ConfigureAwait(false);

        if (mapping.TryGetValue(tld, out var url))
        {
            return url;
        }

        return null;
    }

    private async Task<Dictionary<string, string>> GetOrLoadBootstrapAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out Dictionary<string, string>? cached) && cached != null)
        {
            return cached;
        }

        _logger.LogDebug("Loading RDAP bootstrap from {Url}", _options.RdapBootstrapUrl);

        try
        {
            var response = await _httpClient.GetStringAsync(
#if NET6_0_OR_GREATER
                _options.RdapBootstrapUrl, cancellationToken
#else
                _options.RdapBootstrapUrl
#endif
            ).ConfigureAwait(false);

            var mapping = ParseBootstrap(response);

            _cache.Set(CacheKey, mapping, _options.RdapBootstrapCacheDuration);

            _logger.LogDebug("Loaded RDAP bootstrap with {Count} entries", mapping.Count);
            return mapping;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to load RDAP bootstrap from {Url}", _options.RdapBootstrapUrl);
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Parses the IANA bootstrap JSON into a TLD → URL dictionary.
    /// </summary>
    internal static Dictionary<string, string> ParseBootstrap(string json)
    {
        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("services", out var services))
        {
            return mapping;
        }

        foreach (var service in services.EnumerateArray())
        {
            if (service.GetArrayLength() < 2)
            {
                continue;
            }

            var tlds = service[0];
            var urls = service[1];

            if (tlds.ValueKind != JsonValueKind.Array || urls.ValueKind != JsonValueKind.Array || urls.GetArrayLength() == 0)
            {
                continue;
            }

            var url = urls[0].GetString();
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            // Ensure trailing slash
            if (!url!.EndsWith("/", StringComparison.Ordinal))
            {
                url += "/";
            }

            foreach (var tld in tlds.EnumerateArray())
            {
                var tldStr = tld.GetString();
                if (!string.IsNullOrWhiteSpace(tldStr))
                {
                    mapping[tldStr!] = url;
                }
            }
        }

        return mapping;
    }
}
