using DevWorker.WhoisNET.Whois.Parsers;

namespace DevWorker.WhoisNET.Tests.Unit.Parsers;

public class EuWhoisParserTests
{
    private readonly EuWhoisParser _parser = new();

    [Fact]
    public void Parse_EuropaEu_ExtractsFields()
    {
        var raw = File.ReadAllText("TestData/whois-responses/europa.eu.txt");

        var result = _parser.Parse(raw, "europa.eu");

        result.Tld.Should().Be("eu");
        result.Registrar.Should().NotBeNull();
        result.Registrar!.Name.Should().Contain("EURid");
        result.NameServers.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Statuses.Should().Contain(s => s.Code == "REGISTERED");
    }

    [Fact]
    public void SupportedTlds_ContainsEu()
    {
        _parser.SupportedTlds.Should().Contain("eu");
    }
}
