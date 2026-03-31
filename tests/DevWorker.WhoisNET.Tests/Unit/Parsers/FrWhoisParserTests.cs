using DevWorker.WhoisNET.Whois.Parsers;

namespace DevWorker.WhoisNET.Tests.Unit.Parsers;

public class FrWhoisParserTests
{
    private readonly FrWhoisParser _parser = new();

    [Fact]
    public void Parse_AfnicFr_ExtractsAllFields()
    {
        var raw = File.ReadAllText("TestData/whois-responses/afnic.fr.txt");

        var result = _parser.Parse(raw, "afnic.fr");

        result.DomainName.Should().Be("afnic.fr");
        result.Tld.Should().Be("fr");
        result.CreatedDate.Should().NotBeNull();
        result.CreatedDate!.Value.Year.Should().Be(1995);
        result.ExpirationDate.Should().NotBeNull();
        result.Registrar.Should().NotBeNull();
        result.Registrar!.Name.Should().Be("AFNIC registry");
        result.NameServers.Should().HaveCountGreaterThanOrEqualTo(3);
        result.NameServers.Should().Contain(ns => ns.HostName == "ns1.nic.fr");
        result.Statuses.Should().Contain(s => s.Code == "ACTIVE");
    }

    [Fact]
    public void SupportedTlds_ContainsFr()
    {
        _parser.SupportedTlds.Should().Contain("fr");
    }
}
