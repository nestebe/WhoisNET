using System.Text;
using DevWorker.WhoisNET.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DevWorker.WhoisNET.Whois;

/// <summary>
/// Handles WHOIS referral chains (thin → thick responses).
/// </summary>
internal sealed class WhoisReferralHandler
{
    private readonly WhoisTcpClient _tcpClient;
    private readonly ILogger _logger;

    internal WhoisReferralHandler(WhoisTcpClient tcpClient, ILogger? logger = null)
    {
        _tcpClient = tcpClient;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Detects a referral server from a WHOIS response.
    /// </summary>
    internal static string? DetectReferral(string response)
    {
        foreach (var line in response.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("Registrar WHOIS Server:", StringComparison.OrdinalIgnoreCase))
            {
                var server = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
                if (!string.IsNullOrWhiteSpace(server))
                {
                    // Remove any protocol prefix
                    server = server.Replace("http://", "").Replace("https://", "").TrimEnd('/');
                    return server;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Follows referral chains to get the thick WHOIS response.
    /// </summary>
    internal async Task<WhoisRawResponse> FollowReferralsAsync(
        string domain,
        string initialServer,
        string initialResponse,
        int maxReferrals,
        Encoding? encoding,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        var currentResponse = initialResponse;
        var currentServer = initialServer;
        var referralCount = 0;

        while (referralCount < maxReferrals)
        {
            var referralServer = DetectReferral(currentResponse);
            if (referralServer == null || referralServer.Equals(currentServer, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            referralCount++;
            _logger.LogDebug("Following WHOIS referral #{Count}: {FromServer} → {ToServer}",
                referralCount, currentServer, referralServer);

            var query = WhoisQueryFormatter.FormatQuery(domain, referralServer);
            var referralResponse = await _tcpClient.QueryAsync(
                referralServer, query, encoding, timeout, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(referralResponse))
            {
                _logger.LogDebug("Referral to {Server} returned empty response, using previous", referralServer);
                break;
            }

            currentServer = referralServer;
            currentResponse = referralResponse;
        }

        return new WhoisRawResponse
        {
            Server = currentServer,
            Content = currentResponse,
            ReferralServer = referralCount > 0 ? currentServer : null,
            IsReferral = referralCount > 0
        };
    }
}
