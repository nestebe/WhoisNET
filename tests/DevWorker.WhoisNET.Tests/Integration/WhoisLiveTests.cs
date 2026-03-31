namespace DevWorker.WhoisNET.Tests.Integration;

[Trait("Category", "Integration")]
public class WhoisLiveTests
{
    [Theory]
    [InlineData("google.com")]
    [InlineData("google.fr")]
    [InlineData("google.org")]
    public async Task Lookup_RealDomain_ReturnsValidResult(string domain)
    {
        using var client = new DomainLookupClient();
        var result = await client.LookupAsync(domain);

        result.DomainName.Should().NotBeNullOrWhiteSpace();
        result.NameServers.Should().NotBeEmpty();
    }
}
