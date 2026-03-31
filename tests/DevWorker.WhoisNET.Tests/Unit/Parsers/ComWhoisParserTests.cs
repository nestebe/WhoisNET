using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Whois.Parsers;

namespace DevWorker.WhoisNET.Tests.Unit.Parsers;

public class ComWhoisParserTests
{
    private readonly ComWhoisParser _parser = new();

    [Fact]
    public void Parse_GoogleCom_ExtractsAllFields()
    {
        var raw = File.ReadAllText("TestData/whois-responses/google.com.txt");

        var result = _parser.Parse(raw, "google.com");

        result.DomainName.Should().NotBeNullOrWhiteSpace();
        result.Registrar.Should().NotBeNull();
        result.Registrar!.Name.Should().Be("MarkMonitor Inc.");
        result.Registrar.IanaId.Should().Be("292");
        result.Registrar.AbuseContactEmail.Should().Be("abusecomplaints@markmonitor.com");
        result.CreatedDate.Should().NotBeNull();
        result.CreatedDate!.Value.Year.Should().Be(1997);
        result.ExpirationDate.Should().NotBeNull();
        result.ExpirationDate!.Value.Year.Should().Be(2028);
        result.NameServers.Should().HaveCountGreaterThanOrEqualTo(4);
        result.Statuses.Should().Contain(s => s.Code == "clientDeleteProhibited");
        result.Dnssec.Should().NotBeNull();
        result.Dnssec!.IsSigned.Should().BeFalse();
    }

    [Fact]
    public void Parse_NotFound_ThrowsDomainNotFoundException()
    {
        var raw = File.ReadAllText("TestData/whois-responses/notfound.com.txt");
        var act = () => _parser.Parse(raw, "xyznotexist12345.com");
        act.Should().Throw<DomainNotFoundException>();
    }

    [Fact]
    public void Parse_RateLimited_ThrowsRateLimitException()
    {
        var raw = File.ReadAllText("TestData/whois-responses/rate-limited.com.txt");
        var act = () => _parser.Parse(raw, "example.com");
        act.Should().Throw<WhoisRateLimitException>();
    }

    [Fact]
    public void SupportedTlds_ContainsComAndNet()
    {
        _parser.SupportedTlds.Should().Contain("com");
        _parser.SupportedTlds.Should().Contain("net");
    }
}
