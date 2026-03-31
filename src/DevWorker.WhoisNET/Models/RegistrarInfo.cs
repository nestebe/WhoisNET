namespace DevWorker.WhoisNET.Models;

/// <summary>
/// Information about the domain registrar.
/// </summary>
public sealed record RegistrarInfo
{
    /// <summary>Registrar name.</summary>
    public string? Name { get; init; }

    /// <summary>IANA registrar ID.</summary>
    public string? IanaId { get; init; }

    /// <summary>Registrar URL.</summary>
    public string? Url { get; init; }

    /// <summary>Abuse contact email.</summary>
    public string? AbuseContactEmail { get; init; }

    /// <summary>Abuse contact phone.</summary>
    public string? AbuseContactPhone { get; init; }

    /// <summary>Registrar's WHOIS server.</summary>
    public string? WhoisServer { get; init; }
}
