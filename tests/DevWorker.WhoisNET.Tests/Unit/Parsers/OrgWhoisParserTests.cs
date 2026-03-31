using DevWorker.WhoisNET.Whois.Parsers;

namespace DevWorker.WhoisNET.Tests.Unit.Parsers;

public class OrgWhoisParserTests
{
    private readonly OrgWhoisParser _parser = new();

    [Fact]
    public void Parse_WikipediaOrg_ExtractsAllFields()
    {
        var raw = File.ReadAllText("TestData/whois-responses/wikipedia.org.txt");

        var result = _parser.Parse(raw, "wikipedia.org");

        result.DomainName.Should().NotBeNullOrWhiteSpace();
        result.Tld.Should().Be("org");
        result.Registrar.Should().NotBeNull();
        result.Registrar!.Name.Should().Be("MarkMonitor Inc.");
        result.CreatedDate.Should().NotBeNull();
        result.CreatedDate!.Value.Year.Should().Be(2001);
        result.ExpirationDate.Should().NotBeNull();
        result.NameServers.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Statuses.Should().NotBeEmpty();
    }

    [Fact]
    public void SupportedTlds_ContainsOrg()
    {
        _parser.SupportedTlds.Should().Contain("org");
    }
}
