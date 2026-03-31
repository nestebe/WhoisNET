using DevWorker.WhoisNET.Internals;

namespace DevWorker.WhoisNET.Tests.Unit;

public class TldHelperTests
{
    [Theory]
    [InlineData("example.com", "com")]
    [InlineData("example.net", "net")]
    [InlineData("example.org", "org")]
    [InlineData("example.fr", "fr")]
    [InlineData("example.de", "de")]
    [InlineData("example.io", "io")]
    [InlineData("EXAMPLE.COM", "com")]
    [InlineData("example.com.", "com")]
    [InlineData(" example.com ", "com")]
    public void GetTld_SimpleTld_ReturnsCorrectTld(string domain, string expected)
    {
        var tld = TldHelper.GetTld(domain);
        tld.Should().Be(expected);
    }

    [Theory]
    [InlineData("bbc.co.uk", "co.uk")]
    [InlineData("example.org.uk", "org.uk")]
    [InlineData("example.com.au", "com.au")]
    [InlineData("example.co.jp", "co.jp")]
    [InlineData("example.com.br", "com.br")]
    public void GetTld_MultiLevelTld_ReturnsCorrectTld(string domain, string expected)
    {
        var tld = TldHelper.GetTld(domain);
        tld.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GetTld_NullOrEmpty_ThrowsArgumentException(string? domain)
    {
        var act = () => TldHelper.GetTld(domain!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetTld_SingleLabel_ThrowsArgumentException()
    {
        var act = () => TldHelper.GetTld("localhost");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("EXAMPLE.COM", "example.com")]
    [InlineData("  example.com.  ", "example.com")]
    [InlineData("Example.Fr", "example.fr")]
    public void NormalizeDomain_ReturnsNormalized(string domain, string expected)
    {
        var result = TldHelper.NormalizeDomain(domain);
        result.Should().Be(expected);
    }
}
