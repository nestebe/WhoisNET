namespace DevWorker.WhoisNET.Exceptions;

/// <summary>
/// Thrown when the queried domain is not found.
/// </summary>
public class DomainNotFoundException : WhoisNetException
{
    /// <summary>The domain that was not found.</summary>
    public string Domain { get; }

    /// <summary>Initializes a new instance of <see cref="DomainNotFoundException"/>.</summary>
    public DomainNotFoundException(string domain)
        : base($"Domain '{domain}' was not found.")
    {
        Domain = domain;
    }

    /// <summary>Initializes a new instance of <see cref="DomainNotFoundException"/>.</summary>
    public DomainNotFoundException(string domain, string message) : base(message)
    {
        Domain = domain;
    }

    /// <summary>Initializes a new instance of <see cref="DomainNotFoundException"/>.</summary>
    public DomainNotFoundException(string domain, string message, Exception innerException) : base(message, innerException)
    {
        Domain = domain;
    }
}
