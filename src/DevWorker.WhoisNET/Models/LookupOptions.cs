using System.Text;

namespace DevWorker.WhoisNET.Models;

/// <summary>
/// Options for a domain lookup request.
/// </summary>
public sealed record LookupOptions
{
    /// <summary>Protocol preference. Auto = RDAP first, then WHOIS.</summary>
    public LookupPreference Preference { get; init; } = LookupPreference.Auto;

    /// <summary>Global request timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);

    /// <summary>Follow WHOIS referrals (thin → thick).</summary>
    public bool FollowReferrals { get; init; } = true;

    /// <summary>Include raw response in the result.</summary>
    public bool IncludeRawResponse { get; init; }

    /// <summary>Bypass cache for this request.</summary>
    public bool BypassCache { get; init; }

    /// <summary>Encoding for WHOIS responses (null = auto-detect).</summary>
    public Encoding? WhoisEncoding { get; init; }

    /// <summary>Maximum number of referrals to follow.</summary>
    public int MaxReferrals { get; init; } = 3;
}
