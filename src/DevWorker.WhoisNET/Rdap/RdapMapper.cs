using System.Text.Json;
using DevWorker.WhoisNET.Internals;
using DevWorker.WhoisNET.Models;

namespace DevWorker.WhoisNET.Rdap;

/// <summary>
/// Maps RDAP responses to <see cref="DomainInfo"/>.
/// </summary>
internal static class RdapMapper
{
    /// <summary>
    /// Maps an <see cref="RdapResponse"/> to a <see cref="DomainInfo"/>.
    /// </summary>
    internal static DomainInfo Map(RdapResponse response, string domain)
    {
        var normalizedDomain = TldHelper.NormalizeDomain(domain);
        var tld = TldHelper.GetTld(normalizedDomain);

        return new DomainInfo
        {
            DomainName = response.LdhName?.ToLowerInvariant() ?? normalizedDomain,
            Tld = tld,
            Protocol = LookupProtocol.Rdap,
            CreatedDate = GetEventDate(response, "registration"),
            ExpirationDate = GetEventDate(response, "expiration"),
            UpdatedDate = GetEventDate(response, "last changed"),
            Registrar = MapRegistrar(response),
            Statuses = MapStatuses(response),
            NameServers = MapNameServers(response),
            Dnssec = MapDnssec(response),
            Registrant = MapContact(response, "registrant"),
            AdminContact = MapContact(response, "administrative"),
            TechContact = MapContact(response, "technical"),
        };
    }

    private static DateTimeOffset? GetEventDate(RdapResponse response, string action)
    {
        if (response.Events == null)
        {
            return null;
        }

        var evt = response.Events.FirstOrDefault(e =>
            string.Equals(e.EventAction, action, StringComparison.OrdinalIgnoreCase));

        return DateParser.TryParse(evt?.EventDate);
    }

    private static RegistrarInfo? MapRegistrar(RdapResponse response)
    {
        if (response.Entities == null)
        {
            return null;
        }

        var registrar = response.Entities.FirstOrDefault(e =>
            e.Roles?.Contains("registrar", StringComparer.OrdinalIgnoreCase) == true);

        if (registrar == null)
        {
            return null;
        }

        var ianaId = registrar.PublicIds?.FirstOrDefault(p =>
            string.Equals(p.Type, "IANA Registrar ID", StringComparison.OrdinalIgnoreCase))?.Identifier;

        var url = registrar.Links?.FirstOrDefault(l =>
            string.Equals(l.Rel, "self", StringComparison.OrdinalIgnoreCase))?.Href;

        var name = ExtractVcardFn(registrar.VcardArray);
        if (name == null && registrar.Handle != null)
        {
            name = registrar.Handle;
        }

        // Look for abuse contact
        string? abuseEmail = null;
        string? abusePhone = null;
        if (registrar.Entities != null)
        {
            var abuse = registrar.Entities.FirstOrDefault(e =>
                e.Roles?.Contains("abuse", StringComparer.OrdinalIgnoreCase) == true);
            if (abuse != null)
            {
                abuseEmail = ExtractVcardEmail(abuse.VcardArray);
                abusePhone = ExtractVcardPhone(abuse.VcardArray);
            }
        }

        return new RegistrarInfo
        {
            Name = name,
            IanaId = ianaId,
            Url = url,
            AbuseContactEmail = abuseEmail,
            AbuseContactPhone = abusePhone,
            WhoisServer = registrar.Port43,
        };
    }

    private static IReadOnlyList<DomainStatus> MapStatuses(RdapResponse response)
    {
        if (response.Status == null)
        {
            return [];
        }

        return response.Status
            .Select(s => new DomainStatus
            {
                Code = s,
                InfoUrl = s.Contains("http") ? null : $"https://icann.org/epp#{s}",
            })
            .ToList();
    }

    private static IReadOnlyList<NameServer> MapNameServers(RdapResponse response)
    {
        if (response.Nameservers == null)
        {
            return [];
        }

        return response.Nameservers
            .Where(ns => ns.LdhName != null)
            .Select(ns =>
            {
                var ips = new List<string>();
                if (ns.IpAddresses?.V4 != null)
                {
                    ips.AddRange(ns.IpAddresses.V4);
                }

                if (ns.IpAddresses?.V6 != null)
                {
                    ips.AddRange(ns.IpAddresses.V6);
                }

                return new NameServer
                {
                    HostName = ns.LdhName!.ToLowerInvariant(),
                    IpAddresses = ips,
                };
            })
            .ToList();
    }

    private static DnssecInfo? MapDnssec(RdapResponse response)
    {
        if (response.SecureDns == null)
        {
            return null;
        }

        var dsData = response.SecureDns.DsData?.FirstOrDefault();
        string? dsString = null;
        if (dsData != null)
        {
            dsString = $"{dsData.KeyTag} {dsData.Algorithm} {dsData.DigestType} {dsData.Digest}";
        }

        return new DnssecInfo
        {
            IsSigned = response.SecureDns.DelegationSigned == true || response.SecureDns.ZoneSigned == true,
            DsData = dsString,
        };
    }

    private static ContactInfo? MapContact(RdapResponse response, string role)
    {
        if (response.Entities == null)
        {
            return null;
        }

        var entity = response.Entities.FirstOrDefault(e =>
            e.Roles?.Contains(role, StringComparer.OrdinalIgnoreCase) == true);

        if (entity == null)
        {
            return null;
        }

        // Check for redaction
        if (entity.Remarks != null)
        {
            var redacted = entity.Remarks.Any(r =>
                r.Title?.Contains("REDACTED", StringComparison.OrdinalIgnoreCase) == true ||
                r.Description?.Any(d => d.Contains("REDACTED", StringComparison.OrdinalIgnoreCase)) == true);

            if (redacted)
            {
                return new ContactInfo { IsRedacted = true };
            }
        }

        return new ContactInfo
        {
            Name = ExtractVcardFn(entity.VcardArray),
            Organization = ExtractVcardOrg(entity.VcardArray),
            Email = ExtractVcardEmail(entity.VcardArray),
            Phone = ExtractVcardPhone(entity.VcardArray),
        };
    }

    // jCard extraction helpers
    // jCard format: ["vcard", [["version", {}, "text", "4.0"], ["fn", {}, "text", "Name"], ...]]

    private static string? ExtractVcardFn(JsonElement? vcardArray) => ExtractVcardField(vcardArray, "fn");

    private static string? ExtractVcardOrg(JsonElement? vcardArray) => ExtractVcardField(vcardArray, "org");

    private static string? ExtractVcardEmail(JsonElement? vcardArray) => ExtractVcardField(vcardArray, "email");

    private static string? ExtractVcardPhone(JsonElement? vcardArray) => ExtractVcardField(vcardArray, "tel");

    private static string? ExtractVcardField(JsonElement? vcardArray, string fieldName)
    {
        if (vcardArray == null || vcardArray.Value.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        try
        {
            var arr = vcardArray.Value;
            if (arr.GetArrayLength() < 2)
            {
                return null;
            }

            var fields = arr[1];
            if (fields.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var field in fields.EnumerateArray())
            {
                if (field.ValueKind != JsonValueKind.Array || field.GetArrayLength() < 4)
                {
                    continue;
                }

                var name = field[0].GetString();
                if (string.Equals(name, fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    var value = field[field.GetArrayLength() - 1];
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        return value.GetString();
                    }
                    else if (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > 0)
                    {
                        // org can be an array: ["org", {}, "text", ["Org Name", ...]]
                        return value[0].GetString();
                    }
                }
            }
        }
        catch (Exception)
        {
            // Malformed jCard — ignore
        }

        return null;
    }
}
