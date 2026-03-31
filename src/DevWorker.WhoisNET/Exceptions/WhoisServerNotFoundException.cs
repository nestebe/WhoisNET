namespace DevWorker.WhoisNET.Exceptions;

/// <summary>
/// Thrown when no WHOIS server can be found for a TLD.
/// </summary>
public class WhoisServerNotFoundException : WhoisNetException
{
    /// <summary>The TLD for which no server was found.</summary>
    public string Tld { get; }

    /// <summary>Initializes a new instance of <see cref="WhoisServerNotFoundException"/>.</summary>
    public WhoisServerNotFoundException(string tld)
        : base($"No WHOIS server found for TLD '{tld}'.")
    {
        Tld = tld;
    }

    /// <summary>Initializes a new instance of <see cref="WhoisServerNotFoundException"/>.</summary>
    public WhoisServerNotFoundException(string tld, string message) : base(message)
    {
        Tld = tld;
    }
}
