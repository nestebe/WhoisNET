namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// WHOIS parser for .io TLD.
/// Uses standard key-value format similar to generic gTLDs.
/// </summary>
internal sealed class IoWhoisParser : GenericWhoisParser
{
    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedTlds => ["io"];
}
