namespace DevWorker.WhoisNET.Exceptions;

/// <summary>
/// Thrown when a WHOIS operation times out.
/// </summary>
public class WhoisTimeoutException : WhoisNetException
{
    /// <summary>Initializes a new instance of <see cref="WhoisTimeoutException"/>.</summary>
    public WhoisTimeoutException() : base("The WHOIS operation timed out.") { }

    /// <summary>Initializes a new instance of <see cref="WhoisTimeoutException"/>.</summary>
    public WhoisTimeoutException(string message) : base(message) { }

    /// <summary>Initializes a new instance of <see cref="WhoisTimeoutException"/>.</summary>
    public WhoisTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
