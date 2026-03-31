namespace DevWorker.WhoisNET.Models;

/// <summary>
/// Contact information (registrant, admin, or tech contact).
/// </summary>
public sealed record ContactInfo
{
    /// <summary>Contact name.</summary>
    public string? Name { get; init; }

    /// <summary>Organization name.</summary>
    public string? Organization { get; init; }

    /// <summary>Street address.</summary>
    public string? Street { get; init; }

    /// <summary>City.</summary>
    public string? City { get; init; }

    /// <summary>State or province.</summary>
    public string? State { get; init; }

    /// <summary>Postal code.</summary>
    public string? PostalCode { get; init; }

    /// <summary>Country code or name.</summary>
    public string? Country { get; init; }

    /// <summary>Email address.</summary>
    public string? Email { get; init; }

    /// <summary>Phone number.</summary>
    public string? Phone { get; init; }

    /// <summary>Indicates whether the data is redacted (GDPR/privacy).</summary>
    public bool IsRedacted { get; init; }
}
