using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// WHOIS parser for .uk TLD (Nominet).
/// Nominet uses a section-based format with indentation.
/// </summary>
internal sealed class UkWhoisParser : IWhoisParser
{
    /// <inheritdoc />
    public IReadOnlyList<string> SupportedTlds => ["uk", "co.uk", "org.uk", "me.uk"];

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

        if (rawResponse.Contains("No match for", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("This domain name has not been registered", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainNotFoundException(domain);
        }

        var normalizedDomain = TldHelper.NormalizeDomain(domain);
        var tld = TldHelper.GetTld(normalizedDomain);

        // Parse section-based format
        var sections = ParseSections(rawResponse);

        string? registrarName = null;
        string? registrarUrl = null;
        DateTimeOffset? createdDate = null;
        DateTimeOffset? expiryDate = null;
        DateTimeOffset? updatedDate = null;
        var nameServers = new List<NameServer>();
        var statuses = new List<DomainStatus>();
        DnssecInfo? dnssec = null;

        // Extract registrar
        if (sections.TryGetValue("Registrar", out var registrarLines))
        {
            foreach (var line in registrarLines)
            {
                if (line.StartsWith("URL:", StringComparison.OrdinalIgnoreCase))
                {
                    registrarUrl = line.Substring(4).Trim();
                }
                else if (registrarName == null && !string.IsNullOrWhiteSpace(line))
                {
                    registrarName = line.Trim();
                }
            }
        }

        // Extract dates
        if (sections.TryGetValue("Relevant dates", out var dateLines))
        {
            foreach (var line in dateLines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Registered on:", StringComparison.OrdinalIgnoreCase))
                {
                    createdDate = DateParser.TryParse(trimmed.Substring("Registered on:".Length).Trim());
                }
                else if (trimmed.StartsWith("Expiry date:", StringComparison.OrdinalIgnoreCase))
                {
                    expiryDate = DateParser.TryParse(trimmed.Substring("Expiry date:".Length).Trim());
                }
                else if (trimmed.StartsWith("Last updated:", StringComparison.OrdinalIgnoreCase))
                {
                    updatedDate = DateParser.TryParse(trimmed.Substring("Last updated:".Length).Trim());
                }
            }
        }

        // Extract registration status
        if (sections.TryGetValue("Registration status", out var statusLines))
        {
            foreach (var line in statusLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    statuses.Add(new DomainStatus { Code = line.Trim() });
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

        // Extract DNSSEC
        if (sections.TryGetValue("DNSSEC", out var dnssecLines))
        {
            var value = dnssecLines.FirstOrDefault()?.Trim() ?? "";
            dnssec = new DnssecInfo
            {
                IsSigned = value.Contains("Signed", StringComparison.OrdinalIgnoreCase)
                           && !value.Contains("unsigned", StringComparison.OrdinalIgnoreCase),
            };
        }

        return new DomainInfo
        {
            DomainName = normalizedDomain,
            Tld = tld,
            Protocol = LookupProtocol.Whois,
            CreatedDate = createdDate,
            ExpirationDate = expiryDate,
            UpdatedDate = updatedDate,
            Registrar = registrarName != null ? new RegistrarInfo { Name = registrarName, Url = registrarUrl } : null,
            Statuses = statuses,
            NameServers = nameServers,
            Dnssec = dnssec,
        };
    }

    /// <summary>
    /// Parses Nominet's section-based format into a dictionary.
    /// Sections are identified by lines that end with ":" and have indented content below.
    /// </summary>
    private static Dictionary<string, List<string>> ParseSections(string rawResponse)
    {
        var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string? currentSection = null;

        foreach (var line in rawResponse.Split('\n'))
        {
            var trimmed = line.TrimEnd();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            // A section header is a line with leading spaces, ending with ':'
            // and the next line is indented more
            var stripped = trimmed.TrimStart();
            if (stripped.EndsWith(":", StringComparison.Ordinal) && !stripped.Contains("http", StringComparison.OrdinalIgnoreCase))
            {
                var indent = trimmed.Length - stripped.Length;
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
