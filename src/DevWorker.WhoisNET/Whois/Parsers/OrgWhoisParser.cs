namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// WHOIS parser for .org TLD (PIR).
/// </summary>
internal sealed class OrgWhoisParser : GenericWhoisParser
{
    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedTlds => ["org"];
}
