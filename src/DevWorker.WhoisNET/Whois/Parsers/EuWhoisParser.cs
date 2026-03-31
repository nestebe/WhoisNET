using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// WHOIS parser for .eu TLD (EURid).
/// EURid uses a section-based format similar to Nominet.
/// </summary>
internal sealed class EuWhoisParser : IWhoisParser
{
    /// <inheritdoc />
    public IReadOnlyList<string> SupportedTlds => ["eu"];

    /// <inheritdoc />
    public DomainInfo Parse(string rawResponse, string domain)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new DomainNotFoundException(domain);
        }

        if (RateLimitDetector.IsRateLimited(rawResponse))
        {
            throw new WhoisRateLimitException($"Rate limited while querying '{domain}'.");
        }

        if (rawResponse.Contains("Status: AVAILABLE", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("No entries found", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainNotFoundException(domain);
        }

        var normalizedDomain = TldHelper.NormalizeDomain(domain);
        var tld = TldHelper.GetTld(normalizedDomain);

        var sections = ParseSections(rawResponse);

        string? registrarName = null;
        string? registrarUrl = null;
        var nameServers = new List<NameServer>();
        var statuses = new List<DomainStatus>();

        // Extract registrar
        if (sections.TryGetValue("Registrar", out var registrarLines))
        {
            foreach (var line in registrarLines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Name:", StringComparison.OrdinalIgnoreCase))
                {
                    registrarName = trimmed.Substring("Name:".Length).Trim();
                }
                else if (trimmed.StartsWith("Website:", StringComparison.OrdinalIgnoreCase))
                {
                    registrarUrl = trimmed.Substring("Website:".Length).Trim();
                }
            }
        }

        // Extract status
        if (sections.TryGetValue("Status", out var statusLines))
        {
            foreach (var line in statusLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    statuses.Add(new DomainStatus { Code = line.Trim() });
                }
            }
        }

        // Also check for inline Status: REGISTERED
        foreach (var line in rawResponse.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("Status:", StringComparison.OrdinalIgnoreCase) && !trimmed.Equals("Status:", StringComparison.OrdinalIgnoreCase))
            {
                var status = trimmed.Substring("Status:".Length).Trim();
                if (!string.IsNullOrWhiteSpace(status) && !statuses.Exists(s => s.Code == status))
                {
                    statuses.Add(new DomainStatus { Code = status });
                }
            }
        }

        // Extract name servers
        if (sections.TryGetValue("Name servers", out var nsLines))
        {
            foreach (var line in nsLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    nameServers.Add(new NameServer { HostName = line.Trim().ToLowerInvariant() });
                }
            }
        }

        return new DomainInfo
        {
            DomainName = normalizedDomain,
            Tld = tld,
            Protocol = LookupProtocol.Whois,
            Registrar = registrarName != null ? new RegistrarInfo { Name = registrarName, Url = registrarUrl } : null,
            Statuses = statuses,
            NameServers = nameServers,
        };
    }

    private static Dictionary<string, List<string>> ParseSections(string rawResponse)
    {
        var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string? currentSection = null;

        foreach (var line in rawResponse.Split('\n'))
        {
            var trimmed = line.TrimEnd();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.TrimStart().StartsWith("%", StringComparison.Ordinal))
            {
                continue;
            }

            var stripped = trimmed.TrimStart();
            if (stripped.EndsWith(":", StringComparison.Ordinal) && !stripped.Contains("http", StringComparison.OrdinalIgnoreCase))
            {
                currentSection = stripped.TrimEnd(':').Trim();
                if (!sections.ContainsKey(currentSection))
                {
                    sections[currentSection] = [];
                }
            }
            else if (currentSection != null)
            {
                sections[currentSection].Add(stripped);
            }
        }

        return sections;
    }
}
