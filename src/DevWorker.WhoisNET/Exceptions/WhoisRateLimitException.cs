namespace DevWorker.WhoisNET.Exceptions;

/// <summary>
/// Thrown when the WHOIS server returns a rate limit response.
/// </summary>
public class WhoisRateLimitException : WhoisNetException
{
    /// <summary>Suggested retry delay, if available.</summary>
    public TimeSpan? RetryAfter { get; init; }

    /// <summary>Initializes a new instance of <see cref="WhoisRateLimitException"/>.</summary>
    public WhoisRateLimitException() : base("WHOIS query rate limit exceeded.") { }

    /// <summary>Initializes a new instance of <see cref="WhoisRateLimitException"/>.</summary>
    public WhoisRateLimitException(string message) : base(message) { }

    /// <summary>Initializes a new instance of <see cref="WhoisRateLimitException"/>.</summary>
    public WhoisRateLimitException(string message, Exception innerException) : base(message, innerException) { }
}
