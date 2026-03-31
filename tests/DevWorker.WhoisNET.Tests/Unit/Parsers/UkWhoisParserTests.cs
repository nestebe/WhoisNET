using DevWorker.WhoisNET.Whois.Parsers;

namespace DevWorker.WhoisNET.Tests.Unit.Parsers;

public class UkWhoisParserTests
{
    private readonly UkWhoisParser _parser = new();

    [Fact]
    public void Parse_BbcCoUk_ExtractsFields()
    {
        var raw = File.ReadAllText("TestData/whois-responses/bbc.co.uk.txt");

        var result = _parser.Parse(raw, "bbc.co.uk");

        result.DomainName.Should().NotBeNullOrWhiteSpace();
        result.Tld.Should().Be("co.uk");
        result.Registrar.Should().NotBeNull();
        result.ExpirationDate.Should().NotBeNull();
        result.ExpirationDate!.Value.Year.Should().Be(2025);
        result.NameServers.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void SupportedTlds_ContainsUkVariants()
    {
        _parser.SupportedTlds.Should().Contain("uk");
        _parser.SupportedTlds.Should().Contain("co.uk");
    }
}
