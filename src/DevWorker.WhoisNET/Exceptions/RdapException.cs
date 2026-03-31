using System.Net;

namespace DevWorker.WhoisNET.Exceptions;

/// <summary>
/// Thrown when an RDAP operation fails.
/// </summary>
public class RdapException : WhoisNetException
{
    /// <summary>HTTP status code returned by the RDAP server.</summary>
    public HttpStatusCode? StatusCode { get; init; }

    /// <summary>Initializes a new instance of <see cref="RdapException"/>.</summary>
    public RdapException() : base("RDAP operation failed.") { }

    /// <summary>Initializes a new instance of <see cref="RdapException"/>.</summary>
    public RdapException(string message) : base(message) { }

    /// <summary>Initializes a new instance of <see cref="RdapException"/>.</summary>
    public RdapException(string message, Exception innerException) : base(message, innerException) { }
}
