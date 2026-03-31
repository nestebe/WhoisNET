namespace DevWorker.WhoisNET.Whois.Parsers;

/// <summary>
/// WHOIS parser for .com and .net TLDs (Verisign).
/// </summary>
internal sealed class ComWhoisParser : GenericWhoisParser
{
    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedTlds => ["com", "net"];
}
