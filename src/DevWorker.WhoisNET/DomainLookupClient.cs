using System.Text.Json;
using DevWorker.WhoisNET.Cache;
using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;
using DevWorker.WhoisNET.Whois.Parsers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DevWorker.WhoisNET;

/// <summary>
/// Main domain lookup client. Uses RDAP first with WHOIS fallback.
/// </summary>
public sealed class DomainLookupClient : IDomainLookupClient
{
    private readonly IWhoisClient _whoisClient;
    private readonly IRdapClient _rdapClient;
    private readonly ILookupCache _cache;
    private readonly WhoisNetOptions _options;
    private readonly ILogger<DomainLookupClient> _logger;
    private readonly HttpClient? _ownedHttpClient;
    private readonly MemoryCache? _ownedMemoryCache;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance for standalone usage (no DI).
    /// </summary>
    public DomainLookupClient()
    {
        _options = new WhoisNetOptions();
        _logger = NullLogger<DomainLookupClient>.Instance;
        _ownedHttpClient = new HttpClient();
        _ownedMemoryCache = new MemoryCache(new MemoryCacheOptions());
        _whoisClient = new WhoisClient();
        _rdapClient = new RdapClient(_ownedHttpClient, _ownedMemoryCache);
        _cache = new MemoryLookupCache(_ownedMemoryCache);
    }

    /// <summary>
    /// Initializes a new instance for DI usage.
    /// </summary>
    public DomainLookupClient(
        IWhoisClient whoisClient,
        IRdapClient rdapClient,
        ILookupCache cache,
        IOptions<WhoisNetOptions>? options = null,
        ILogger<DomainLookupClient>? logger = null)
    {
        _whoisClient = whoisClient;
        _rdapClient = rdapClient;
        _cache = cache;
        _options = options?.Value ?? new WhoisNetOptions();
        _logger = logger ?? NullLogger<DomainLookupClient>.Instance;
    }

    /// <inheritdoc />
    public async Task<DomainInfo> LookupAsync(
        string domain,
        LookupOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        domain = TldHelper.NormalizeDomain(domain);
        options ??= new LookupOptions();

        // Check cache
        if (!options.BypassCache)
        {
            var cached = await _cache.GetAsync(domain, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for {Domain}", domain);
                return cached;
            }
        }

        DomainInfo result;

        switch (options.Preference)
        {
            case LookupPreference.RdapOnly:
                result = await LookupRdapAsync(domain, options, cancellationToken).ConfigureAwait(false);
                break;

            case LookupPreference.WhoisOnly:
                result = await LookupWhoisAsync(domain, options, cancellationToken).ConfigureAwait(false);
                break;

            case LookupPreference.Auto:
            default:
                result = await LookupAutoAsync(domain, options, cancellationToken).ConfigureAwait(false);
                break;
        }

        // Store in cache
        await _cache.SetAsync(domain, result, _options.CacheDuration, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(string domain, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            await LookupAsync(domain, new LookupOptions { BypassCache = true }, cancellationToken).ConfigureAwait(false);
            return false;
        }
        catch (DomainNotFoundException)
        {
            return true;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetRawWhoisAsync(string domain, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var response = await _whoisClient.QueryAsync(domain, cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Content;
    }

    /// <inheritdoc />
    public async Task<JsonDocument?> GetRawRdapAsync(string domain, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var response = await _rdapClient.GetRawResponseAsync(domain, cancellationToken).ConfigureAwait(false);
        if (response == null)
        {
            return null;
        }

        var json = JsonSerializer.Serialize(response);
        return JsonDocument.Parse(json);
    }

    private async Task<DomainInfo> LookupAutoAsync(string domain, LookupOptions options, CancellationToken cancellationToken)
    {
        // Try RDAP first
        try
        {
            return await LookupRdapAsync(domain, options, cancellationToken).ConfigureAwait(false);
        }
        catch (DomainNotFoundException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "RDAP failed for {Domain}, falling back to WHOIS", domain);
        }

        // Fallback to WHOIS
        return await LookupWhoisAsync(domain, options, cancellationToken).ConfigureAwait(false);
    }

    private async Task<DomainInfo> LookupRdapAsync(string domain, LookupOptions options, CancellationToken cancellationToken)
    {
        var result = await _rdapClient.LookupAsync(domain, cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            throw new RdapException($"RDAP is not available for domain '{domain}'.");
        }

        if (options.IncludeRawResponse)
        {
            var raw = await _rdapClient.GetRawResponseAsync(domain, cancellationToken).ConfigureAwait(false);
            if (raw != null)
            {
                result = result with { RawRdapResponse = JsonSerializer.Serialize(raw) };
            }
        }

        return result;
    }

    private async Task<DomainInfo> LookupWhoisAsync(string domain, LookupOptions options, CancellationToken cancellationToken)
    {
        var rawResponse = await _whoisClient.QueryAsync(domain, cancellationToken: cancellationToken).ConfigureAwait(false);

        // Parse the response
        var parser = WhoisParserFactory.GetParser(domain);
        var result = parser.Parse(rawResponse.Content, domain);

        if (options.IncludeRawResponse)
        {
            result = result with { RawWhoisResponse = rawResponse.Content };
        }

        return result;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _ownedHttpClient?.Dispose();
        _ownedMemoryCache?.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DomainLookupClient));
        }
    }
}
