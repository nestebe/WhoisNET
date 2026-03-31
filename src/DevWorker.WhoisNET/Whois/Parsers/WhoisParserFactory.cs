using DevWorker.WhoisNET.Internals;

namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// Factory for selecting the appropriate WHOIS parser for a TLD.
/// </summary>
internal sealed class WhoisParserFactory
{
    private static readonly IWhoisParser[] SpecializedParsers =
    [
        new ComWhoisParser(),
        new FrWhoisParser(),
        new OrgWhoisParser(),
        new DeWhoisParser(),
        new UkWhoisParser(),
        new EuWhoisParser(),
        new IoWhoisParser(),
    ];

    private static readonly GenericWhoisParser FallbackParser = new();

    /// <summary>
    /// Gets the most appropriate parser for the given domain.
    /// </summary>
    internal static IWhoisParser GetParser(string domain)
    {
        var tld = TldHelper.GetTld(domain);
        return GetParserForTld(tld);
    }

    /// <summary>
    /// Gets the most appropriate parser for the given TLD.
    /// </summary>
    internal static IWhoisParser GetParserForTld(string tld)
    {
        tld = tld.ToLowerInvariant();

        foreach (var parser in SpecializedParsers)
        {
            foreach (var supported in parser.SupportedTlds)
            {
                if (supported.Equals(tld, StringComparison.OrdinalIgnoreCase))
                {
                    return parser;
                }
            }
        }

        return FallbackParser;
    }
}
