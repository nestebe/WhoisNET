namespace DevWorker.WhoisNET.Models;

/// <summary>
/// Structured domain information returned by a lookup.
/// </summary>
public sealed record DomainInfo
{
    /// <summary>Fully qualified domain name (e.g., "example.com").</summary>
    public required string DomainName { get; init; }

    /// <summary>Extracted TLD (e.g., "com", "co.uk").</summary>
    public required string Tld { get; init; }

    /// <summary>Protocol used to obtain the data.</summary>
    public required LookupProtocol Protocol { get; init; }

    // --- Dates ---

    /// <summary>Domain creation date.</summary>
    public DateTimeOffset? CreatedDate { get; init; }

    /// <summary>Domain expiration date.</summary>
    public DateTimeOffset? ExpirationDate { get; init; }

    /// <summary>Last update date.</summary>
    public DateTimeOffset? UpdatedDate { get; init; }

    // --- Registrar ---

    /// <summary>Registrar information.</summary>
    public RegistrarInfo? Registrar { get; init; }

    // --- Status ---

    /// <summary>EPP status codes.</summary>
    public IReadOnlyList<DomainStatus> Statuses { get; init; } = [];

    // --- DNS ---

    /// <summary>Name servers.</summary>
    public IReadOnlyList<NameServer> NameServers { get; init; } = [];

    /// <summary>DNSSEC information.</summary>
    public DnssecInfo? Dnssec { get; init; }

    // --- Contacts (may be empty due to GDPR) ---

    /// <summary>Registrant contact.</summary>
    public ContactInfo? Registrant { get; init; }

    /// <summary>Administrative contact.</summary>
    public ContactInfo? AdminContact { get; init; }

    /// <summary>Technical contact.</summary>
    public ContactInfo? TechContact { get; init; }

    // --- Raw ---

    /// <summary>Raw WHOIS server response (text).</summary>
    public string? RawWhoisResponse { get; init; }

    /// <summary>Raw RDAP server response (JSON).</summary>
    public string? RawRdapResponse { get; init; }

    /// <summary>Timestamp of the query.</summary>
    public DateTimeOffset QueryTimestamp { get; init; } = DateTimeOffset.UtcNow;
}
