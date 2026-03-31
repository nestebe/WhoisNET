using System.Text.Json;
using DevWorker.WhoisNET.Models;
using DevWorker.WhoisNET.Rdap;

namespace DevWorker.WhoisNET.Tests.Unit;

public class RdapMapperTests
{
    [Fact]
    public void Map_GoogleCom_ExtractsAllFields()
    {
        var json = File.ReadAllText("TestData/rdap-responses/google.com.json");
        var response = JsonSerializer.Deserialize<RdapResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = RdapMapper.Map(response, "google.com");

        result.DomainName.Should().Be("google.com");
        result.Tld.Should().Be("com");
        result.Protocol.Should().Be(LookupProtocol.Rdap);
        result.CreatedDate.Should().NotBeNull();
        result.CreatedDate!.Value.Year.Should().Be(1997);
        result.ExpirationDate.Should().NotBeNull();
        result.ExpirationDate!.Value.Year.Should().Be(2028);
        result.UpdatedDate.Should().NotBeNull();
        result.Registrar.Should().NotBeNull();
        result.Registrar!.Name.Should().Be("MarkMonitor Inc.");
        result.Registrar.IanaId.Should().Be("292");
        result.Registrar.AbuseContactEmail.Should().Be("abusecomplaints@markmonitor.com");
        result.NameServers.Should().HaveCount(4);
        result.Statuses.Should().HaveCountGreaterThan(0);
        result.Dnssec.Should().NotBeNull();
        result.Dnssec!.IsSigned.Should().BeFalse();
    }

    [Fact]
    public void Map_AfnicFr_ExtractsFields()
    {
        var json = File.ReadAllText("TestData/rdap-responses/afnic.fr.json");
        var response = JsonSerializer.Deserialize<RdapResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = RdapMapper.Map(response, "afnic.fr");

        result.DomainName.Should().Be("afnic.fr");
        result.Tld.Should().Be("fr");
        result.CreatedDate.Should().NotBeNull();
        result.ExpirationDate.Should().NotBeNull();
        result.NameServers.Should().HaveCount(3);
        result.NameServers.Should().Contain(ns => ns.IpAddresses.Count > 0);
        result.Dnssec!.IsSigned.Should().BeTrue();
    }

    [Fact]
    public void Map_WikipediaOrg_ExtractsRegistrant()
    {
        var json = File.ReadAllText("TestData/rdap-responses/wikipedia.org.json");
        var response = JsonSerializer.Deserialize<RdapResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = RdapMapper.Map(response, "wikipedia.org");

        result.DomainName.Should().Be("wikipedia.org");
        result.Registrant.Should().NotBeNull();
        result.Registrant!.Organization.Should().Be("Wikimedia Foundation, Inc.");
    }
}
