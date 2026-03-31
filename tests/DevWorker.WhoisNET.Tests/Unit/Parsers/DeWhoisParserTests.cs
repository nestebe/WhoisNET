using DevWorker.WhoisNET.Whois.Parsers;

namespace DevWorker.WhoisNET.Tests.Unit.Parsers;

public class DeWhoisParserTests
{
    private readonly DeWhoisParser _parser = new();

    [Fact]
    public void Parse_DenicDe_ExtractsFields()
    {
        var raw = File.ReadAllText("TestData/whois-responses/denic.de.txt");

        var result = _parser.Parse(raw, "denic.de");

        result.DomainName.Should().Be("denic.de");
        result.Tld.Should().Be("de");
        result.NameServers.Should().HaveCountGreaterThanOrEqualTo(3);
        result.NameServers.Should().Contain(ns => ns.HostName == "ns1.denic.de");
        result.Statuses.Should().Contain(s => s.Code == "connect");
    }

    [Fact]
    public void SupportedTlds_ContainsDe()
    {
        _parser.SupportedTlds.Should().Contain("de");
    }
}
