namespace DevWorker.WhoisNET.Exceptions;

/// <summary>
/// Thrown when parsing a WHOIS response fails.
/// </summary>
public class WhoisParsingException : WhoisNetException
{
    /// <summary>The raw WHOIS response that failed to parse.</summary>
    public string RawResponse { get; }

    /// <summary>Initializes a new instance of <see cref="WhoisParsingException"/>.</summary>
    public WhoisParsingException(string rawResponse)
        : base("Failed to parse WHOIS response.")
    {
        RawResponse = rawResponse;
    }

    /// <summary>Initializes a new instance of <see cref="WhoisParsingException"/>.</summary>
    public WhoisParsingException(string rawResponse, string message) : base(message)
    {
        RawResponse = rawResponse;
    }

    /// <summary>Initializes a new instance of <see cref="WhoisParsingException"/>.</summary>
    public WhoisParsingException(string rawResponse, string message, Exception innerException) : base(message, innerException)
    {
        RawResponse = rawResponse;
    }
}
