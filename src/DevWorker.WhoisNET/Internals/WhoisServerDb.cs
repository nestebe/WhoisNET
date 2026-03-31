#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace DevWorker.WhoisNET.Internals;

/// <summary>
/// Static database mapping TLDs to their WHOIS servers.
/// </summary>
internal static class WhoisServerDb
{
#if NET8_0_OR_GREATER
    internal static readonly FrozenDictionary<string, string> Servers =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // gTLDs
            ["com"] = "whois.verisign-grs.com",
            ["net"] = "whois.verisign-grs.com",
            ["org"] = "whois.pir.org",
            ["info"] = "whois.afilias.net",
            ["biz"] = "whois.biz",
            ["name"] = "whois.nic.name",
            ["mobi"] = "whois.afilias.net",

            // New gTLDs
            ["dev"] = "whois.nic.google",
            ["app"] = "whois.nic.google",
            ["io"] = "whois.nic.io",
            ["ai"] = "whois.nic.ai",
            ["co"] = "whois.nic.co",
            ["me"] = "whois.nic.me",
            ["tv"] = "whois.nic.tv",
            ["xyz"] = "whois.nic.xyz",
            ["online"] = "whois.nic.online",
            ["tech"] = "whois.nic.tech",
            ["site"] = "whois.nic.site",
            ["store"] = "whois.nic.store",
            ["cloud"] = "whois.nic.cloud",

            // ccTLDs Europe
            ["fr"] = "whois.nic.fr",
            ["de"] = "whois.denic.de",
            ["uk"] = "whois.nic.uk",
            ["co.uk"] = "whois.nic.uk",
            ["eu"] = "whois.eu",
            ["nl"] = "whois.domain-registry.nl",
            ["be"] = "whois.dns.be",
            ["it"] = "whois.nic.it",
            ["es"] = "whois.nic.es",
            ["pt"] = "whois.dns.pt",
            ["ch"] = "whois.nic.ch",
            ["at"] = "whois.nic.at",
            ["pl"] = "whois.dns.pl",
            ["se"] = "whois.iis.se",
            ["no"] = "whois.norid.no",
            ["dk"] = "whois.dk-hostmaster.dk",
            ["fi"] = "whois.fi",
            ["ie"] = "whois.weare.ie",
            ["cz"] = "whois.nic.cz",
            ["ru"] = "whois.tcinet.ru",

            // ccTLDs Americas
            ["us"] = "whois.nic.us",
            ["ca"] = "whois.cira.ca",
            ["br"] = "whois.registro.br",
            ["mx"] = "whois.mx",

            // ccTLDs Asia-Pacific
            ["jp"] = "whois.jprs.jp",
            ["cn"] = "whois.cnnic.cn",
            ["au"] = "whois.auda.org.au",
            ["nz"] = "whois.srs.net.nz",
            ["in"] = "whois.registry.in",
            ["kr"] = "whois.kr",
            ["tw"] = "whois.twnic.net.tw",
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
#else
    internal static readonly IReadOnlyDictionary<string, string> Servers =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // gTLDs
            ["com"] = "whois.verisign-grs.com",
            ["net"] = "whois.verisign-grs.com",
            ["org"] = "whois.pir.org",
            ["info"] = "whois.afilias.net",
            ["biz"] = "whois.biz",
            ["name"] = "whois.nic.name",
            ["mobi"] = "whois.afilias.net",

            // New gTLDs
            ["dev"] = "whois.nic.google",
            ["app"] = "whois.nic.google",
            ["io"] = "whois.nic.io",
            ["ai"] = "whois.nic.ai",
            ["co"] = "whois.nic.co",
            ["me"] = "whois.nic.me",
            ["tv"] = "whois.nic.tv",
            ["xyz"] = "whois.nic.xyz",
            ["online"] = "whois.nic.online",
            ["tech"] = "whois.nic.tech",
            ["site"] = "whois.nic.site",
            ["store"] = "whois.nic.store",
            ["cloud"] = "whois.nic.cloud",

            // ccTLDs Europe
            ["fr"] = "whois.nic.fr",
            ["de"] = "whois.denic.de",
            ["uk"] = "whois.nic.uk",
            ["co.uk"] = "whois.nic.uk",
            ["eu"] = "whois.eu",
            ["nl"] = "whois.domain-registry.nl",
            ["be"] = "whois.dns.be",
            ["it"] = "whois.nic.it",
            ["es"] = "whois.nic.es",
            ["pt"] = "whois.dns.pt",
            ["ch"] = "whois.nic.ch",
            ["at"] = "whois.nic.at",
            ["pl"] = "whois.dns.pl",
            ["se"] = "whois.iis.se",
            ["no"] = "whois.norid.no",
            ["dk"] = "whois.dk-hostmaster.dk",
            ["fi"] = "whois.fi",
            ["ie"] = "whois.weare.ie",
            ["cz"] = "whois.nic.cz",
            ["ru"] = "whois.tcinet.ru",

            // ccTLDs Americas
            ["us"] = "whois.nic.us",
            ["ca"] = "whois.cira.ca",
            ["br"] = "whois.registro.br",
            ["mx"] = "whois.mx",

            // ccTLDs Asia-Pacific
            ["jp"] = "whois.jprs.jp",
            ["cn"] = "whois.cnnic.cn",
            ["au"] = "whois.auda.org.au",
            ["nz"] = "whois.srs.net.nz",
            ["in"] = "whois.registry.in",
            ["kr"] = "whois.kr",
            ["tw"] = "whois.twnic.net.tw",
        };
#endif
}
