# WhoisNET

A modern, structured **WHOIS** and **RDAP** client for .NET.

Supports all major TLDs (`.com`, `.fr`, `.org`, `.de`, `.uk`, `.eu`, `.io`, etc.) with strongly-typed results, RDAP-first with WHOIS fallback, async-first API, and DI support.

## Features

- **RDAP-first with WHOIS fallback** — structured JSON when available, raw TCP when not
- **Strongly-typed results** — `DomainInfo`, `RegistrarInfo`, `ContactInfo`, `NameServer`, etc.
- **Multi-TLD support** — specialized parsers for `.com`, `.fr`, `.de`, `.uk`, `.eu`, `.org`, `.io`
- **Async-first** — all operations are async with `CancellationToken` support
- **DI-friendly** — `IServiceCollection.AddWhoisNet()` extension
- **Standalone usage** — works without DI via `new DomainLookupClient()`
- **Built-in caching** — in-memory cache with configurable TTL
- **WHOIS referral handling** — follows thin-to-thick referral chains
- **Multi-targeting** — `netstandard2.0`, `net6.0`, `net8.0`, `net10.0`
- **Zero third-party dependencies** — Microsoft packages only

## Installation

```
dotnet add package DevWorker.WhoisNET
```

## Quick Start

```csharp
using DevWorker.WhoisNET;

using var client = new DomainLookupClient();

var result = await client.LookupAsync("google.com");
Console.WriteLine($"Registrar: {result.Registrar?.Name}");
Console.WriteLine($"Expires:   {result.ExpirationDate}");
Console.WriteLine($"NS:        {string.Join(", ", result.NameServers.Select(n => n.HostName))}");
```

## Usage with DI

```csharp
// In Startup / Program.cs
services.AddWhoisNet(options =>
{
    options.CacheDuration = TimeSpan.FromMinutes(30);
    options.RdapUserAgent = "MyApp/1.0";
});

// Inject IDomainLookupClient
public class MyService(IDomainLookupClient client)
{
    public async Task CheckDomain(string domain)
    {
        var info = await client.LookupAsync(domain);
        // ...
    }
}
```

## Configuration

| Option | Default | Description |
|---|---|---|
| `CacheDuration` | 15 min | Default cache TTL |
| `WhoisTimeout` | 10s | WHOIS TCP timeout |
| `RdapTimeout` | 10s | RDAP HTTP timeout |
| `RdapUserAgent` | `WhoisNET/1.0` | User-Agent for RDAP |
| `RdapBootstrapUrl` | IANA URL | RDAP bootstrap endpoint |
| `CustomServers` | empty | Custom TLD-to-server mappings |

## Lookup Options

```csharp
var result = await client.LookupAsync("example.fr", new LookupOptions
{
    Preference = LookupPreference.WhoisOnly,  // or RdapOnly, Auto (default)
    IncludeRawResponse = true,
    BypassCache = true,
    Timeout = TimeSpan.FromSeconds(20),
});
```

## Check Domain Availability

```csharp
bool available = await client.IsAvailableAsync("mynewdomain.com");
```

## Raw Responses

```csharp
// Raw WHOIS text
string whois = await client.GetRawWhoisAsync("example.com");

// Raw RDAP JSON
JsonDocument? rdap = await client.GetRawRdapAsync("example.com");
```

## TLDs with Specialized Parsers

| TLD | Registry | Parser |
|---|---|---|
| `.com`, `.net` | Verisign | `ComWhoisParser` |
| `.org` | PIR | `OrgWhoisParser` |
| `.fr` | AFNIC | `FrWhoisParser` |
| `.de` | DENIC | `DeWhoisParser` |
| `.uk`, `.co.uk` | Nominet | `UkWhoisParser` |
| `.eu` | EURid | `EuWhoisParser` |
| `.io` | NIC.io | `IoWhoisParser` |

All other TLDs use the `GenericWhoisParser` (key-value format).

## Authors 

[Nicolas ESTEBE](https://github.com/kevinwairi)

## License

MIT
