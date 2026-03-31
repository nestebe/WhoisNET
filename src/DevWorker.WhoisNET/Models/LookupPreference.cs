namespace DevWorker.WhoisNET.Models;

/// <summary>
/// Preferred protocol for domain lookups.
/// </summary>
public enum LookupPreference
{
    /// <summary>RDAP first, fallback to WHOIS.</summary>
    Auto = 0,

    /// <summary>WHOIS only.</summary>
    WhoisOnly = 1,

    /// <summary>RDAP only.</summary>
    RdapOnly = 2
}
