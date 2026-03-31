using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Whois.Parsers;

namespace DevWorker.WhoisNET.Tests.Unit.Parsers;

public class GenericWhoisParserTests
{
    private readonly GenericWhoisParser _parser = new();

    [Fact]
    public void Parse_EmptyResponse_ThrowsDomainNotFoundException()
    {
        var act = () => _parser.Parse("", "example.com");
        act.Should().Throw<DomainNotFoundException>();
    }

    [Fact]
    public void Parse_NotFoundResponse_ThrowsDomainNotFoundException()
    {
        var act = () => _parser.Parse("No match for \"EXAMPLE.COM\".", "example.com");
        act.Should().Throw<DomainNotFoundException>();
    }

    [Fact]
    public void Parse_RateLimitedResponse_ThrowsWhoisRateLimitException()
    {
        var act = () => _parser.Parse("Query rate limit exceeded. Please try again later.", "example.com");
        act.Should().Throw<WhoisRateLimitException>();
    }

    [Fact]
    public void Parse_ValidResponse_ExtractsBasicFields()
    {
        var raw = """
            Domain Name: EXAMPLE.COM
            Creation Date: 2000-01-01T00:00:00Z
            Registry Expiry Date: 2030-01-01T00:00:00Z
            Registrar: Example Registrar
            Name Server: NS1.EXAMPLE.COM
            Name Server: NS2.EXAMPLE.COM
            DNSSEC: unsigned
            """;

        var result = _parser.Parse(raw, "example.com");

        result.DomainName.Should().Be("EXAMPLE.COM");
        result.CreatedDate.Should().NotBeNull();
        result.ExpirationDate.Should().NotBeNull();
        result.Registrar.Should().NotBeNull();
        result.Registrar!.Name.Should().Be("Example Registrar");
        result.NameServers.Should().HaveCount(2);
        result.Dnssec.Should().NotBeNull();
        result.Dnssec!.IsSigned.Should().BeFalse();
    }

    [Fact]
    public void Parse_MultipleStatuses_ExtractsAll()
    {
        var raw = """
            Domain Name: EXAMPLE.COM
            Domain Status: clientDeleteProhibited https://icann.org/epp#clientDeleteProhibited
            Domain Status: clientTransferProhibited https://icann.org/epp#clientTransferProhibited
            Registrar: Test
            """;

        var result = _parser.Parse(raw, "example.com");

        result.Statuses.Should().HaveCount(2);
        result.Statuses[0].Code.Should().Be("clientDeleteProhibited");
        result.Statuses[1].Code.Should().Be("clientTransferProhibited");
    }
}
