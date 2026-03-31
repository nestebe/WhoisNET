namespace DevWorker.WhoisNET.Models;

/// <summary>
/// Protocol used for the domain lookup.
/// </summary>
public enum LookupProtocol
{
    /// <summary>Unknown protocol.</summary>
    Unknown = 0,

    /// <summary>WHOIS (RFC 3912, TCP port 43).</summary>
    Whois = 1,

    /// <summary>RDAP (RFC 7480-7484, REST/JSON).</summary>
    Rdap = 2
}
