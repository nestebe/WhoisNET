namespace DevWorker.WhoisNET.Models;

/// <summary>
/// Global configuration options for WhoisNET.
/// </summary>
public sealed class WhoisNetOptions
{
    /// <summary>Default cache duration.</summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Default timeout for WHOIS TCP connections.</summary>
    public TimeSpan WhoisTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Default timeout for RDAP HTTP requests.</summary>
    public TimeSpan RdapTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>User-Agent header for RDAP requests.</summary>
    public string RdapUserAgent { get; set; } = "WhoisNET/1.0";

    /// <summary>IANA RDAP bootstrap URL.</summary>
    public string RdapBootstrapUrl { get; set; } = "https://data.iana.org/rdap/dns.json";

    /// <summary>Cache duration for the RDAP bootstrap data.</summary>
    public TimeSpan RdapBootstrapCacheDuration { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Enable detailed request logging.</summary>
    public bool EnableDetailedLogging { get; set; }

    /// <summary>Custom WHOIS server overrides (TLD → server).</summary>
    public Dictionary<string, string> CustomServers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
