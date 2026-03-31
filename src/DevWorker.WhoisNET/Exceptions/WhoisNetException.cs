namespace DevWorker.WhoisNET.Exceptions;

/// <summary>
/// Base exception for all WhoisNET errors.
/// </summary>
public class WhoisNetException : Exception
{
    /// <summary>Initializes a new instance of <see cref="WhoisNetException"/>.</summary>
    public WhoisNetException() { }

    /// <summary>Initializes a new instance of <see cref="WhoisNetException"/>.</summary>
    public WhoisNetException(string message) : base(message) { }

    /// <summary>Initializes a new instance of <see cref="WhoisNetException"/>.</summary>
    public WhoisNetException(string message, Exception innerException) : base(message, innerException) { }
}
