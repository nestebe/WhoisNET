using DevWorker.WhoisNET.Internals;

namespace DevWorker.WhoisNET.Tests.Unit;

public class WhoisServerResolverTests
{
    [Theory]
    [InlineData("com", "whois.verisign-grs.com")]
    [InlineData("net", "whois.verisign-grs.com")]
    [InlineData("org", "whois.pir.org")]
    [InlineData("fr", "whois.nic.fr")]
    [InlineData("de", "whois.denic.de")]
    [InlineData("uk", "whois.nic.uk")]
    [InlineData("eu", "whois.eu")]
    [InlineData("io", "whois.nic.io")]
    [InlineData("dev", "whois.nic.google")]
    [InlineData("app", "whois.nic.google")]
    [InlineData("co.uk", "whois.nic.uk")]
    public void WhoisServerDb_ContainsKnownTlds(string tld, string expectedServer)
    {
        WhoisServerDb.Servers.Should().ContainKey(tld);
        WhoisServerDb.Servers[tld].Should().Be(expectedServer);
    }

    [Fact]
    public void WhoisServerDb_IsCaseInsensitive()
    {
        WhoisServerDb.Servers.Should().ContainKey("COM");
        WhoisServerDb.Servers.Should().ContainKey("Fr");
    }

    [Fact]
    public void WhoisServerDb_HasMinimumEntries()
    {
        WhoisServerDb.Servers.Should().HaveCountGreaterThanOrEqualTo(40);
    }
}
