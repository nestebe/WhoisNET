namespace DevWorker.WhoisNET.Models;

/// <summary>
/// Raw WHOIS response with metadata.
/// </summary>
public sealed record WhoisRawResponse
{
    /// <summary>The WHOIS server that provided the response.</summary>
    public required string Server { get; init; }

    /// <summary>Raw text content of the response.</summary>
    public required string Content { get; init; }

    /// <summary>Referral server if present in the response.</summary>
    public string? ReferralServer { get; init; }

    /// <summary>Whether this response was the result of following a referral.</summary>
    public bool IsReferral { get; init; }
}
