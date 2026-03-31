namespace DevWorker.WhoisNET.Internals;

/// <summary>
/// Helper for extracting TLDs from domain names.
/// </summary>
internal static class TldHelper
{
    // Known second-level TLDs (e.g., co.uk, com.au)
    private static readonly HashSet<string> SecondLevelTlds = new(StringComparer.OrdinalIgnoreCase)
    {
        "co.uk", "org.uk", "me.uk", "net.uk", "ltd.uk", "plc.uk",
        "com.au", "net.au", "org.au", "edu.au", "gov.au",
        "co.nz", "net.nz", "org.nz",
        "co.jp", "or.jp", "ne.jp", "ac.jp",
        "com.br", "net.br", "org.br",
        "com.mx", "net.mx", "org.mx",
        "co.kr", "or.kr", "ne.kr",
        "com.cn", "net.cn", "org.cn",
        "com.tw", "net.tw", "org.tw",
        "co.in", "net.in", "org.in",
        "com.ru", "net.ru", "org.ru",
        "co.za", "net.za", "org.za",
        "com.ar", "net.ar", "org.ar",
        "com.pl", "net.pl", "org.pl",
    };

    /// <summary>
    /// Extracts the TLD from a domain name.
    /// Handles multi-level TLDs like "co.uk".
    /// </summary>
    /// <param name="domain">The domain name (e.g., "example.co.uk").</param>
    /// <returns>The TLD (e.g., "co.uk" or "com").</returns>
    internal static string GetTld(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new ArgumentException("Domain cannot be null or empty.", nameof(domain));
        }

        domain = domain.Trim().TrimEnd('.').ToLowerInvariant();

        var parts = domain.Split('.');
        if (parts.Length < 2)
        {
            throw new ArgumentException($"Invalid domain name: '{domain}'.", nameof(domain));
        }

        // Check for second-level TLD (e.g., co.uk)
        if (parts.Length >= 3)
        {
            var candidate = parts[parts.Length - 2] + "." + parts[parts.Length - 1];
            if (SecondLevelTlds.Contains(candidate))
            {
                return candidate;
            }
        }

        return parts[parts.Length - 1];
    }

    /// <summary>
    /// Normalizes a domain name to lowercase, trimmed, without trailing dot.
    /// </summary>
    internal static string NormalizeDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new ArgumentException("Domain cannot be null or empty.", nameof(domain));
        }

        return domain.Trim().TrimEnd('.').ToLowerInvariant();
    }
}
