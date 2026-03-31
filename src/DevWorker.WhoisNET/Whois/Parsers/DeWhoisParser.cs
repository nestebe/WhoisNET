using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// WHOIS parser for .de TLD (DENIC).
/// DENIC uses a multi-section format with [...] headers.
/// Does not expose expiration dates.
/// </summary>
internal sealed class DeWhoisParser : GenericWhoisParser
{
    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedTlds => ["de"];

    /// <inheritdoc />
    protected override void CheckNotFound(string rawResponse, string domain)
    {
        if (rawResponse.Contains("Status: free", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("% Object \"" + domain + "\" not found", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("No entries found", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainNotFoundException(domain);
        }
    }

    /// <inheritdoc />
    protected override DomainInfo MapToDomainInfo(Dictionary<string, List<string>> data, string domain, string rawResponse)
    {
        var normalizedDomain = TldHelper.NormalizeDomain(domain);
        var tld = TldHelper.GetTld(normalizedDomain);

        var statuses = GetAll(data, "Status", "Domain Status");

        return new DomainInfo
        {
            DomainName = GetFirst(data, "Domain", "Domain Name") ?? normalizedDomain,
            Tld = tld,
            Protocol = LookupProtocol.Whois,
            CreatedDate = DateParser.TryParse(GetFirst(data, "Changed", "Registered")),
            UpdatedDate = DateParser.TryParse(GetFirst(data, "Changed")),
            Statuses = statuses.Select(s => new DomainStatus { Code = s.Trim() }).ToList(),
            NameServers = ParseNameServers(data),
            Dnssec = ParseDnssec(data),
        };
    }

    private static DnssecInfo? ParseDnssec(Dictionary<string, List<string>> data)
    {
        var value = GetFirst(data, "DNSSEC", "Dnskey");
        if (value == null)
        {
            return null;
        }

        return new DnssecInfo { IsSigned = !string.IsNullOrWhiteSpace(value) };
    }

    /// <inheritdoc />
    protected override IReadOnlyList<NameServer> ParseNameServers(Dictionary<string, List<string>> data)
    {
        var raw = GetAll(data, "Nserver", "Name Server", "nserver");
        return raw
            .Where(ns => !string.IsNullOrWhiteSpace(ns))
            .Select(ns =>
            {
                var parts = ns.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new NameServer
                {
                    HostName = parts[0].ToLowerInvariant(),
                    IpAddresses = parts.Length > 1 ? parts.Skip(1).ToList() : [],
                };
            })
            .ToList();
    }
}
