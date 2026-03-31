using System.Globalization;

namespace DevWorker.WhoisNET.Internals;

/// <summary>
/// Multi-format date parser for WHOIS and RDAP responses.
/// </summary>
internal static class DateParser
{
    private static readonly string[] Formats =
    [
        // ISO 8601
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.fZ",
        "yyyy-MM-ddTHH:mm:ss.ffZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:sszzz",
        "yyyy-MM-ddTHH:mm:ss.fzzz",
        "yyyy-MM-ddTHH:mm:ss.ffzzz",
        "yyyy-MM-ddTHH:mm:ss.fffzzz",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd",

        // Common WHOIS formats
        "dd-MMM-yyyy",              // 15-Jan-2025 (Nominet UK)
        "dd/MM/yyyy",               // 15/01/2025 (AFNIC FR)
        "MM/dd/yyyy",               // 01/15/2025 (US)
        "yyyy/MM/dd",               // 2025/01/15 (JP)
        "yyyyMMdd",                 // 20250115 (compact)

        // With time
        "dd-MMM-yyyy HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss (UTC+0)",
        "yyyy-MM-dd HH:mm:ss+00",
        "dd/MM/yyyy HH:mm:ss",
    ];

    /// <summary>
    /// Attempts to parse a date string in any known format.
    /// </summary>
    /// <param name="value">The date string to parse.</param>
    /// <returns>A <see cref="DateTimeOffset"/> if parsing succeeds; otherwise, <c>null</c>.</returns>
    internal static DateTimeOffset? TryParse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value!.Trim();

        // Try standard DateTimeOffset parsing first (handles most ISO 8601 variants)
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out var result))
        {
            return result;
        }

        // Try specific formats
        if (DateTimeOffset.TryParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out result))
        {
            return result;
        }

        return null;
    }
}
