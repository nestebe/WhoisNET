namespace DevWorker.WhoisNET.Models;

/// <summary>
/// EPP domain status code.
/// </summary>
public sealed record DomainStatus
{
    /// <summary>EPP status code (e.g., "clientTransferProhibited").</summary>
    public required string Code { get; init; }

    /// <summary>Human-readable description.</summary>
    public string? Description { get; init; }

    /// <summary>ICANN documentation URL for this status.</summary>
    public string? InfoUrl { get; init; }
}
