using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// Interface for parsing raw WHOIS responses into structured data.
/// </summary>
public interface IWhoisParser
{
    /// <summary>TLDs supported by this parser.</summary>
    IReadOnlyList<string> SupportedTlds { get; }

    /// <summary>Parses a raw WHOIS response into a <see cref="DomainInfo"/>.</summary>
    /// <param name="rawResponse">Raw WHOIS text response.</param>
    /// <param name="domain">The queried domain name.</param>
    /// <returns>Structured domain information.</returns>
    DomainInfo Parse(string rawResponse, string domain);
}
