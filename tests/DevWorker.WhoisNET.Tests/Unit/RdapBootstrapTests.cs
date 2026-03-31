using DevWorker.WhoisNET.Rdap;

namespace DevWorker.WhoisNET.Tests.Unit;

public class RdapBootstrapTests
{
    private const string SampleBootstrap = """
        {
          "version": "1.0",
          "publication": "2024-01-01T00:00:00Z",
          "services": [
            [["com", "net"], ["https://rdap.verisign.com/com/v1/"]],
            [["fr"], ["https://rdap.nic.fr/"]],
            [["org"], ["https://rdap.org/"]]
          ]
        }
        """;

    [Fact]
    public void ParseBootstrap_ValidJson_ReturnsMappings()
    {
        var result = RdapBootstrap.ParseBootstrap(SampleBootstrap);

        result.Should().ContainKey("com");
        result.Should().ContainKey("net");
        result.Should().ContainKey("fr");
        result.Should().ContainKey("org");
    }

    [Fact]
    public void ParseBootstrap_ComAndNet_MapToVerisign()
    {
        var result = RdapBootstrap.ParseBootstrap(SampleBootstrap);

        result["com"].Should().StartWith("https://rdap.verisign.com/");
        result["net"].Should().StartWith("https://rdap.verisign.com/");
    }

    [Fact]
    public void ParseBootstrap_Fr_MapsToAfnic()
    {
        var result = RdapBootstrap.ParseBootstrap(SampleBootstrap);

        result["fr"].Should().Be("https://rdap.nic.fr/");
    }

    [Fact]
    public void ParseBootstrap_UrlsHaveTrailingSlash()
    {
        var result = RdapBootstrap.ParseBootstrap(SampleBootstrap);

        foreach (var url in result.Values)
        {
            url.Should().EndWith("/");
        }
    }

    [Fact]
    public void ParseBootstrap_EmptyJson_ReturnsEmptyDictionary()
    {
        var result = RdapBootstrap.ParseBootstrap("{}");
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseBootstrap_IsCaseInsensitive()
    {
        var result = RdapBootstrap.ParseBootstrap(SampleBootstrap);

        result.Should().ContainKey("COM");
        result.Should().ContainKey("Fr");
    }
}
