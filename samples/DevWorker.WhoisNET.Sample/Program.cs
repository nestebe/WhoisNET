using DevWorker.WhoisNET;
using DevWorker.WhoisNET.Models;

Console.WriteLine("=== WhoisNET Sample ===\n");

using var client = new DomainLookupClient();

// Simple lookup
var domains = new[] { "google.com", "google.fr", "wikipedia.org", "dev-worker.com", "1parrainage.com" };

foreach (var domain in domains)
{
    try
    {
        Console.WriteLine($"--- {domain} ---");
        var result = await client.LookupAsync(domain, new LookupOptions
        {
            Preference = LookupPreference.Auto,
        });

        Console.WriteLine($"  Domain:     {result.DomainName}");
        Console.WriteLine($"  TLD:        {result.Tld}");
        Console.WriteLine($"  Protocol:   {result.Protocol}");
        Console.WriteLine($"  Registrar:  {result.Registrar?.Name ?? "N/A"}");
        Console.WriteLine($"  Created:    {result.CreatedDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        Console.WriteLine($"  Expires:    {result.ExpirationDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        Console.WriteLine($"  NS:         {string.Join(", ", result.NameServers.Select(n => n.HostName))}");
        Console.WriteLine($"  Statuses:   {string.Join(", ", result.Statuses.Select(s => s.Code))}");
        Console.WriteLine($"  DNSSEC:     {(result.Dnssec?.IsSigned == true ? "Signed" : "Unsigned")}");
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}\n");
    }
}

// Check availability
Console.WriteLine("--- Availability Check ---");
var available = await client.IsAvailableAsync("thisisaverylongdomainthatdoesnotexist12345.com");
Console.WriteLine($"  thisisaverylongdomainthatdoesnotexist12345.com: {(available ? "Available" : "Taken")}");

Console.WriteLine("\nDone.");
