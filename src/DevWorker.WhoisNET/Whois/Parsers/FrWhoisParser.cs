using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// WHOIS parser for .fr TLD (AFNIC).
/// </summary>
internal sealed class FrWhoisParser : GenericWhoisParser
{
    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedTlds => ["fr"];

    /// <inheritdoc />
    protected override void CheckNotFound(string rawResponse, string domain)
    {
        if (rawResponse.Contains("No entries found", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("%%ERROR", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("Status:        AVAILABLE", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainNotFoundException(domain);
        }

        base.CheckNotFound(rawResponse, domain);
    }

    /// <inheritdoc />
    protected override DomainInfo MapToDomainInfo(Dictionary<string, List<string>> data, string domain, string rawResponse)
    {
        var normalizedDomain = TldHelper.NormalizeDomain(domain);
        var tld = TldHelper.GetTld(normalizedDomain);

        // AFNIC uses different keys
        var statuses = GetAll(data, "status");

        return new DomainInfo
        {
            DomainName = GetFirst(data, "domain") ?? normalizedDomain,
            Tld = tld,
            Protocol = LookupProtocol.Whois,
            CreatedDate = DateParser.TryParse(GetFirst(data, "created", "Creation Date")),
            ExpirationDate = DateParser.TryParse(GetFirst(data, "Expiry Date", "expires")),
            UpdatedDate = DateParser.TryParse(GetFirst(data, "last-update", "Updated Date")),
            Registrar = ParseFrRegistrar(data),
            Statuses = statuses.Select(s => new DomainStatus { Code = s.Trim() }).ToList(),
            NameServers = ParseNameServers(data),
            Dnssec = ParseFrDnssec(rawResponse),
        };
    }

    /// <inheritdoc />
    protected override IReadOnlyList<NameServer> ParseNameServers(Dictionary<string, List<string>> data)
    {
        var raw = GetAll(data, "nserver", "Name Server");
        return raw
            .Where(ns => !string.IsNullOrWhiteSpace(ns))
            .Select(ns =>
            {
                // AFNIC format: "ns1.example.com 1.2.3.4" or just "ns1.example.com"
                var parts = ns.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new NameServer
                {
                    HostName = parts[0].ToLowerInvariant(),
                    IpAddresses = parts.Length > 1 ? parts.Skip(1).ToList() : [],
                };
            })
            .ToList();
    }

    private static RegistrarInfo? ParseFrRegistrar(Dictionary<string, List<string>> data)
    {
        var name = GetFirst(data, "registrar");
        if (name == null)
        {
            return null;
        }

        return new RegistrarInfo { Name = name };
    }

    private static DnssecInfo? ParseFrDnssec(string rawResponse)
    {
        // Look for "DNSSEC: signedDelegation" or similar
        foreach (var line in rawResponse.Split('\n'))
        {
            if (line.Trim().StartsWith("DNSSEC:", StringComparison.OrdinalIgnoreCase))
            {
                var value = line.Substring(line.IndexOf(':') + 1).Trim();
                return new DnssecInfo
                {
                    IsSigned = value.Contains("signed", StringComparison.OrdinalIgnoreCase),
                };
            }
        }

        return null;
    }
}
