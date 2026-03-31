using System.Text;
using DevWorker.WhoisNET.Models;
using DevWorker.WhoisNET.Whois;
using DevWorker.WhoisNET.Whois.Parsers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DevWorker.WhoisNET;

/// <summary>
/// WHOIS client facade that handles server resolution, TCP queries, referrals, and parsing.
/// </summary>
public sealed class WhoisClient : IWhoisClient
{
    private readonly WhoisTcpClient _tcpClient;
    private readonly WhoisServerResolver _serverResolver;
    private readonly WhoisReferralHandler _referralHandler;
    private readonly WhoisParserFactory _parserFactory;
    private readonly WhoisNetOptions _options;
    private readonly ILogger<WhoisClient> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="WhoisClient"/> for DI usage.
    /// </summary>
    public WhoisClient(
        IOptions<WhoisNetOptions>? options = null,
        ILogger<WhoisClient>? logger = null)
    {
        _options = options?.Value ?? new WhoisNetOptions();
        _logger = logger ?? NullLogger<WhoisClient>.Instance;
        _tcpClient = new WhoisTcpClient(_logger);
        _serverResolver = new WhoisServerResolver(_tcpClient, options, _logger);
        _referralHandler = new WhoisReferralHandler(_tcpClient, _logger);
        _parserFactory = new WhoisParserFactory();
    }

    /// <inheritdoc />
    public async Task<WhoisRawResponse> QueryAsync(
        string domain,
        string? whoisServer = null,
        CancellationToken cancellationToken = default)
    {
        domain = Internals.TldHelper.NormalizeDomain(domain);

        var server = whoisServer
            ?? await _serverResolver.GetServerForDomainAsync(domain, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Querying WHOIS for {Domain} via {Server}", domain, server);

        var query = WhoisQueryFormatter.FormatQuery(domain, server);
        var response = await _tcpClient.QueryAsync(
            server, query, null, _options.WhoisTimeout, cancellationToken).ConfigureAwait(false);

        // Follow referrals
        var result = await _referralHandler.FollowReferralsAsync(
            domain, server, response, 3, null, _options.WhoisTimeout, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Queries WHOIS and parses the response into structured data.
    /// </summary>
    internal async Task<DomainInfo> LookupAsync(
        string domain,
        LookupOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        domain = Internals.TldHelper.NormalizeDomain(domain);
        options ??= new LookupOptions();

        var server = await _serverResolver.GetServerForDomainAsync(domain, cancellationToken).ConfigureAwait(false);
        var query = WhoisQueryFormatter.FormatQuery(domain, server);

        var response = await _tcpClient.QueryAsync(
            server, query, options.WhoisEncoding, options.Timeout, cancellationToken).ConfigureAwait(false);

        // Follow referrals if enabled
        WhoisRawResponse rawResponse;
        if (options.FollowReferrals)
        {
            rawResponse = await _referralHandler.FollowReferralsAsync(
                domain, server, response, options.MaxReferrals, options.WhoisEncoding, options.Timeout, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            rawResponse = new WhoisRawResponse { Server = server, Content = response };
        }

        // Parse the response
        var parser = WhoisParserFactory.GetParser(domain);
        var result = parser.Parse(rawResponse.Content, domain);

        if (options.IncludeRawResponse)
        {
            result = result with { RawWhoisResponse = rawResponse.Content };
        }

        return result;
    }
}
