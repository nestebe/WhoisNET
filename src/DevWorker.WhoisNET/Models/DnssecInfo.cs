namespace DevWorker.WhoisNET.Models;

/// <summary>
/// DNSSEC information for a domain.
/// </summary>
public sealed record DnssecInfo
{
    /// <summary>Whether DNSSEC is enabled (signed).</summary>
    public bool IsSigned { get; init; }

    /// <summary>DS record data if available.</summary>
    public string? DsData { get; init; }
}
