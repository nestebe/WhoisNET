using System.Text.Json.Serialization;

namespace DevWorker.WhoisNET.Rdap;

/// <summary>
/// RDAP domain response per RFC 9083.
/// </summary>
public sealed class RdapResponse
{
    /// <summary>Object class name (should be "domain").</summary>
    [JsonPropertyName("objectClassName")]
    public string? ObjectClassName { get; set; }

    /// <summary>LDH name (ASCII domain name).</summary>
    [JsonPropertyName("ldhName")]
    public string? LdhName { get; set; }

    /// <summary>Unicode name.</summary>
    [JsonPropertyName("unicodeName")]
    public string? UnicodeName { get; set; }

    /// <summary>Handle (registry-assigned ID).</summary>
    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    /// <summary>Status codes.</summary>
    [JsonPropertyName("status")]
    public List<string>? Status { get; set; }

    /// <summary>Events (registration, expiration, last changed).</summary>
    [JsonPropertyName("events")]
    public List<RdapEvent>? Events { get; set; }

    /// <summary>Entities (registrar, registrant, tech, admin).</summary>
    [JsonPropertyName("entities")]
    public List<RdapEntity>? Entities { get; set; }

    /// <summary>Name servers.</summary>
    [JsonPropertyName("nameservers")]
    public List<RdapNameServer>? Nameservers { get; set; }

    /// <summary>Secure DNS information.</summary>
    [JsonPropertyName("secureDNS")]
    public RdapSecureDns? SecureDns { get; set; }

    /// <summary>Links.</summary>
    [JsonPropertyName("links")]
    public List<RdapLink>? Links { get; set; }

    /// <summary>Notices.</summary>
    [JsonPropertyName("notices")]
    public List<RdapNotice>? Notices { get; set; }

    /// <summary>Port 43 WHOIS server.</summary>
    [JsonPropertyName("port43")]
    public string? Port43 { get; set; }
}

/// <summary>
/// RDAP event (registration, expiration, etc.).
/// </summary>
public sealed class RdapEvent
{
    /// <summary>Event action (registration, expiration, last changed, etc.).</summary>
    [JsonPropertyName("eventAction")]
    public string? EventAction { get; set; }

    /// <summary>Event date.</summary>
    [JsonPropertyName("eventDate")]
    public string? EventDate { get; set; }
}

/// <summary>
/// RDAP entity (registrar, registrant, etc.).
/// </summary>
public sealed class RdapEntity
{
    /// <summary>Object class name.</summary>
    [JsonPropertyName("objectClassName")]
    public string? ObjectClassName { get; set; }

    /// <summary>Handle.</summary>
    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    /// <summary>Roles for this entity.</summary>
    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }

    /// <summary>Public IDs.</summary>
    [JsonPropertyName("publicIds")]
    public List<RdapPublicId>? PublicIds { get; set; }

    /// <summary>vCard array (jCard format).</summary>
    [JsonPropertyName("vcardArray")]
    public System.Text.Json.JsonElement? VcardArray { get; set; }

    /// <summary>Entities nested within this entity.</summary>
    [JsonPropertyName("entities")]
    public List<RdapEntity>? Entities { get; set; }

    /// <summary>Events.</summary>
    [JsonPropertyName("events")]
    public List<RdapEvent>? Events { get; set; }

    /// <summary>Links.</summary>
    [JsonPropertyName("links")]
    public List<RdapLink>? Links { get; set; }

    /// <summary>Remarks.</summary>
    [JsonPropertyName("remarks")]
    public List<RdapNotice>? Remarks { get; set; }

    /// <summary>Status.</summary>
    [JsonPropertyName("status")]
    public List<string>? Status { get; set; }

    /// <summary>Port 43 WHOIS server.</summary>
    [JsonPropertyName("port43")]
    public string? Port43 { get; set; }
}

/// <summary>
/// RDAP public ID (e.g., IANA registrar ID).
/// </summary>
public sealed class RdapPublicId
{
    /// <summary>ID type.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>ID value.</summary>
    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }
}

/// <summary>
/// RDAP name server.
/// </summary>
public sealed class RdapNameServer
{
    /// <summary>Object class name.</summary>
    [JsonPropertyName("objectClassName")]
    public string? ObjectClassName { get; set; }

    /// <summary>LDH name.</summary>
    [JsonPropertyName("ldhName")]
    public string? LdhName { get; set; }

    /// <summary>IP addresses.</summary>
    [JsonPropertyName("ipAddresses")]
    public RdapIpAddresses? IpAddresses { get; set; }
}

/// <summary>
/// RDAP IP addresses for a name server.
/// </summary>
public sealed class RdapIpAddresses
{
    /// <summary>IPv4 addresses.</summary>
    [JsonPropertyName("v4")]
    public List<string>? V4 { get; set; }

    /// <summary>IPv6 addresses.</summary>
    [JsonPropertyName("v6")]
    public List<string>? V6 { get; set; }
}

/// <summary>
/// RDAP secure DNS information.
/// </summary>
public sealed class RdapSecureDns
{
    /// <summary>Whether the zone is signed.</summary>
    [JsonPropertyName("zoneSigned")]
    public bool? ZoneSigned { get; set; }

    /// <summary>Whether delegation is signed.</summary>
    [JsonPropertyName("delegationSigned")]
    public bool? DelegationSigned { get; set; }

    /// <summary>DS data.</summary>
    [JsonPropertyName("dsData")]
    public List<RdapDsData>? DsData { get; set; }
}

/// <summary>
/// RDAP DS data record.
/// </summary>
public sealed class RdapDsData
{
    /// <summary>Key tag.</summary>
    [JsonPropertyName("keyTag")]
    public int? KeyTag { get; set; }

    /// <summary>Algorithm.</summary>
    [JsonPropertyName("algorithm")]
    public int? Algorithm { get; set; }

    /// <summary>Digest type.</summary>
    [JsonPropertyName("digestType")]
    public int? DigestType { get; set; }

    /// <summary>Digest.</summary>
    [JsonPropertyName("digest")]
    public string? Digest { get; set; }
}

/// <summary>
/// RDAP link.
/// </summary>
public sealed class RdapLink
{
    /// <summary>Link value.</summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>Relation type.</summary>
    [JsonPropertyName("rel")]
    public string? Rel { get; set; }

    /// <summary>Link href.</summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    /// <summary>Content type.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

/// <summary>
/// RDAP notice or remark.
/// </summary>
public sealed class RdapNotice
{
    /// <summary>Title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>Description lines.</summary>
    [JsonPropertyName("description")]
    public List<string>? Description { get; set; }

    /// <summary>Links.</summary>
    [JsonPropertyName("links")]
    public List<RdapLink>? Links { get; set; }
}
