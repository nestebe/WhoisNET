using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// Generic WHOIS parser for standard "Key: Value" format responses.
/// </summary>
internal class GenericWhoisParser : IWhoisParser
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> SupportedTlds => [];

    // Not-found patterns
    private static readonly string[] NotFoundPatterns =
    [
        "No match for",
        "NOT FOUND",
        "No Data Found",
        "No entries found",
        "Domain not found",
        "No information available",
        "Status: free",
        "Status: AVAILABLE",
        "is free",
        "No Object Found",
        "Object does not exist",
    ];

    /// <inheritdoc />
    public virtual DomainInfo Parse(string rawResponse, string domain)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new DomainNotFoundException(domain, $"Empty WHOIS response for '{domain}'.");
        }

        if (RateLimitDetector.IsRateLimited(rawResponse))
        {
            throw new WhoisRateLimitException($"Rate limited while querying '{domain}'.");
        }

        CheckNotFound(rawResponse, domain);

        var data = ParseKeyValuePairs(rawResponse);
        return MapToDomainInfo(data, domain, rawResponse);
    }

    /// <summary>
    /// Checks if the response indicates the domain was not found.
    /// </summary>
    protected virtual void CheckNotFound(string rawResponse, string domain)
    {
        foreach (var pattern in NotFoundPatterns)
        {
            if (rawResponse.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainNotFoundException(domain);
            }
        }
    }

    /// <summary>
    /// Parses key-value pairs from a WHOIS response.
    /// Returns a dictionary where keys map to lists of values (for multi-value fields).
    /// </summary>
    protected virtual Dictionary<string, List<string>> ParseKeyValuePairs(string rawResponse)
    {
        var data = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in rawResponse.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed[0] == '%' || trimmed[0] == '#' || trimmed.StartsWith(">>>", StringComparison.Ordinal))
            {
                continue;
            }

            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0 || colonIndex >= trimmed.Length - 1)
            {
                continue;
            }

            var key = trimmed.Substring(0, colonIndex).Trim();
            var value = trimmed.Substring(colonIndex + 1).Trim();

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (!data.TryGetValue(key, out var list))
            {
                list = [];
                data[key] = list;
            }

            list.Add(value);
        }

        return data;
    }

    /// <summary>
    /// Maps parsed key-value data to a <see cref="DomainInfo"/>.
    /// </summary>
    protected virtual DomainInfo MapToDomainInfo(Dictionary<string, List<string>> data, string domain, string rawResponse)
    {
        var normalizedDomain = TldHelper.NormalizeDomain(domain);
        var tld = TldHelper.GetTld(normalizedDomain);

        return new DomainInfo
        {
            DomainName = GetFirst(data, "Domain Name") ?? normalizedDomain,
            Tld = tld,
            Protocol = LookupProtocol.Whois,
            CreatedDate = DateParser.TryParse(GetFirst(data, "Creation Date", "Created Date", "Created", "Registration Date", "created")),
            ExpirationDate = DateParser.TryParse(GetFirst(data, "Registry Expiry Date", "Expiry Date", "Expiration Date", "paid-till", "Expires On", "expires")),
            UpdatedDate = DateParser.TryParse(GetFirst(data, "Updated Date", "Last Updated", "Last Modified", "last-update", "changed")),
            Registrar = ParseRegistrar(data),
            Statuses = ParseStatuses(data),
            NameServers = ParseNameServers(data),
            Dnssec = ParseDnssec(data),
            Registrant = ParseContact(data, "Registrant"),
            AdminContact = ParseContact(data, "Admin"),
            TechContact = ParseContact(data, "Tech"),
        };
    }

    /// <summary>
    /// Gets the first non-null value from multiple possible keys.
    /// </summary>
    protected static string? GetFirst(Dictionary<string, List<string>> data, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (data.TryGetValue(key, out var values) && values.Count > 0)
            {
                return values[0];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all values for any of the given keys.
    /// </summary>
    protected static List<string> GetAll(Dictionary<string, List<string>> data, params string[] keys)
    {
        var result = new List<string>();
        foreach (var key in keys)
        {
            if (data.TryGetValue(key, out var values))
            {
                result.AddRange(values);
            }
        }

        return result;
    }

    private static RegistrarInfo? ParseRegistrar(Dictionary<string, List<string>> data)
    {
        var name = GetFirst(data, "Registrar", "Registrar Name", "Sponsoring Registrar");
        if (name == null)
        {
            return null;
        }

        return new RegistrarInfo
        {
            Name = name,
            IanaId = GetFirst(data, "Registrar IANA ID"),
            Url = GetFirst(data, "Registrar URL"),
            AbuseContactEmail = GetFirst(data, "Registrar Abuse Contact Email"),
            AbuseContactPhone = GetFirst(data, "Registrar Abuse Contact Phone"),
            WhoisServer = GetFirst(data, "Registrar WHOIS Server"),
        };
    }

    private static IReadOnlyList<DomainStatus> ParseStatuses(Dictionary<string, List<string>> data)
    {
        var raw = GetAll(data, "Domain Status", "Status", "status");
        var statuses = new List<DomainStatus>();

        foreach (var entry in raw)
        {
            // Format is typically "clientTransferProhibited https://icann.org/epp#clientTransferProhibited"
            var parts = entry.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            statuses.Add(new DomainStatus
            {
                Code = parts[0],
                InfoUrl = parts.Length > 1 ? parts[1] : null,
            });
        }

        return statuses;
    }

    /// <summary>
    /// Parses name servers from the data.
    /// </summary>
    protected virtual IReadOnlyList<NameServer> ParseNameServers(Dictionary<string, List<string>> data)
    {
        var raw = GetAll(data, "Name Server", "nserver", "Nameservers", "DNS");
        return raw
            .Where(ns => !string.IsNullOrWhiteSpace(ns))
            .Select(ns => new NameServer { HostName = ns.Trim().ToLowerInvariant() })
            .ToList();
    }

    private static DnssecInfo? ParseDnssec(Dictionary<string, List<string>> data)
    {
        var value = GetFirst(data, "DNSSEC", "dnssec");
        if (value == null)
        {
            return null;
        }

        return new DnssecInfo
        {
            IsSigned = value.Contains("signed", StringComparison.OrdinalIgnoreCase)
                       && !value.Contains("unsigned", StringComparison.OrdinalIgnoreCase),
            DsData = GetFirst(data, "DNSSEC DS Data", "DS Data"),
        };
    }

    private static ContactInfo? ParseContact(Dictionary<string, List<string>> data, string prefix)
    {
        var name = GetFirst(data, $"{prefix} Name", $"{prefix} Organization");
        if (name == null)
        {
            // Check for GDPR redaction
            var redacted = GetFirst(data, $"{prefix} Name") ?? GetFirst(data, $"{prefix} Email");
            if (redacted != null && (
                    redacted.Contains("REDACTED", StringComparison.OrdinalIgnoreCase) ||
                    redacted.Contains("Privacy", StringComparison.OrdinalIgnoreCase) ||
                    redacted.Contains("DATA REDACTED", StringComparison.OrdinalIgnoreCase)))
            {
                return new ContactInfo { IsRedacted = true };
            }

            return null;
        }

        return new ContactInfo
        {
            Name = GetFirst(data, $"{prefix} Name"),
            Organization = GetFirst(data, $"{prefix} Organization"),
            Street = GetFirst(data, $"{prefix} Street"),
            City = GetFirst(data, $"{prefix} City"),
            State = GetFirst(data, $"{prefix} State/Province"),
            PostalCode = GetFirst(data, $"{prefix} Postal Code"),
            Country = GetFirst(data, $"{prefix} Country"),
            Email = GetFirst(data, $"{prefix} Email"),
            Phone = GetFirst(data, $"{prefix} Phone"),
        };
    }
}
