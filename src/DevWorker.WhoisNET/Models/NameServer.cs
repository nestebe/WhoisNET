namespace DevWorker.WhoisNET.Models;

/// <summary>
/// DNS name server information.
/// </summary>
public sealed record NameServer
{
    /// <summary>Name server hostname (e.g., "ns1.example.com").</summary>
    public required string HostName { get; init; }

    /// <summary>IP addresses of the name server (glue records).</summary>
    public IReadOnlyList<string> IpAddresses { get; init; } = [];
}
