using System.Net;
using System.Text.Json;
using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;
using DevWorker.WhoisNET.Rdap;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DevWorker.WhoisNET;

/// <summary>
/// RDAP client for HTTP-based domain lookups.
/// </summary>
public sealed class RdapClient : IRdapClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly RdapBootstrap _bootstrap;
    private readonly WhoisNetOptions _options;
    private readonly ILogger<RdapClient> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RdapClient"/> for DI usage.
    /// </summary>
    public RdapClient(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<WhoisNetOptions>? options = null,
        ILogger<RdapClient>? logger = null)
    {
        _httpClient = httpClient;
        _options = options?.Value ?? new WhoisNetOptions();
        _logger = logger ?? NullLogger<RdapClient>.Instance;
        _bootstrap = new RdapBootstrap(httpClient, cache, options, _logger);
    }

    /// <inheritdoc />
    public async Task<DomainInfo?> LookupAsync(string domain, CancellationToken cancellationToken = default)
    {
        var response = await GetRawResponseAsync(domain, cancellationToken).ConfigureAwait(false);
        if (response == null)
        {
            return null;
        }

        return RdapMapper.Map(response, domain);
    }

    /// <inheritdoc />
    public async Task<RdapResponse?> GetRawResponseAsync(string domain, CancellationToken cancellationToken = default)
    {
        domain = TldHelper.NormalizeDomain(domain);
        var tld = TldHelper.GetTld(domain);

        var serverUrl = await _bootstrap.GetServerUrlAsync(tld, cancellationToken).ConfigureAwait(false);
        if (serverUrl == null)
        {
            _logger.LogDebug("No RDAP server available for TLD {Tld}", tld);
            return null;
        }

        var requestUrl = $"{serverUrl}domain/{domain}";
        _logger.LogDebug("RDAP request: GET {Url}", requestUrl);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/rdap+json"));
            if (!string.IsNullOrWhiteSpace(_options.RdapUserAgent))
            {
                request.Headers.UserAgent.ParseAdd(_options.RdapUserAgent);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new DomainNotFoundException(domain, $"RDAP: domain '{domain}' not found.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new RdapException($"RDAP request to {requestUrl} returned {response.StatusCode}.")
                {
                    StatusCode = response.StatusCode,
                };
            }

            var json = await response.Content.ReadAsStringAsync(
#if NET6_0_OR_GREATER
                cancellationToken
#endif
            ).ConfigureAwait(false);

            var rdapResponse = JsonSerializer.Deserialize<RdapResponse>(json, JsonOptions);

            _logger.LogDebug("RDAP response received for {Domain}", domain);
            return rdapResponse;
        }
        catch (Exception ex) when (ex is not WhoisNetException && ex is not OperationCanceledException)
        {
            throw new RdapException($"RDAP request failed for domain '{domain}'.", ex);
        }
    }
}
