namespace DevWorker.WhoisNET.Whois;

/// <summary>
/// Formats WHOIS queries for specific servers that require special syntax.
/// </summary>
internal static class WhoisQueryFormatter
{
    /// <summary>
    /// Formats a WHOIS query string for the given server.
    /// </summary>
    internal static string FormatQuery(string domain, string server)
    {
        return server switch
        {
            "whois.denic.de" => $"-T dn,ace {domain}\r\n",
            "whois.jprs.jp" => $"{domain}/e\r\n",
            "whois.verisign-grs.com" => $"={domain}\r\n",
            _ => $"{domain}\r\n"
        };
    }
}
