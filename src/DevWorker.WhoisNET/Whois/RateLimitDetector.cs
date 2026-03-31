namespace DevWorker.WhoisNET.Whois;

/// <summary>
/// Detects rate limiting responses from WHOIS servers.
/// </summary>
internal static class RateLimitDetector
{
    private static readonly string[] Patterns =
    [
        "rate limit",
        "too many requests",
        "query rate exceeded",
        "try again later",
        "exceeded the maximum",
        "quota exceeded",
        "please try again",
        "connection limit",
    ];

    /// <summary>
    /// Checks whether the response indicates rate limiting.
    /// </summary>
    internal static bool IsRateLimited(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return false;
        }

        foreach (var pattern in Patterns)
        {
            if (response.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
