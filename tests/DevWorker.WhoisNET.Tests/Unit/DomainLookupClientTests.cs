using DevWorker.WhoisNET.Cache;
using DevWorker.WhoisNET.Exceptions;
using DevWorker.WhoisNET.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DevWorker.WhoisNET.Tests.Unit;

public class DomainLookupClientTests
{
    private readonly IWhoisClient _whoisClient = Substitute.For<IWhoisClient>();
    private readonly IRdapClient _rdapClient = Substitute.For<IRdapClient>();
    private readonly ILookupCache _cache = Substitute.For<ILookupCache>();

    [Fact]
    public async Task LookupAsync_CacheHit_ReturnsCachedResult()
    {
        var cached = CreateTestDomainInfo("example.com");
        _cache.GetAsync("example.com", Arg.Any<CancellationToken>())
            .Returns(cached);

        using var client = new DomainLookupClient(_whoisClient, _rdapClient, _cache);
        var result = await client.LookupAsync("example.com");

        result.DomainName.Should().Be("example.com");
        await _rdapClient.DidNotReceive().LookupAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LookupAsync_BypassCache_SkipsCache()
    {
        var rdapResult = CreateTestDomainInfo("example.com", LookupProtocol.Rdap);
        _rdapClient.LookupAsync("example.com", Arg.Any<CancellationToken>())
            .Returns(rdapResult);

        using var client = new DomainLookupClient(_whoisClient, _rdapClient, _cache);
        var result = await client.LookupAsync("example.com", new LookupOptions { BypassCache = true });

        result.Should().NotBeNull();
        await _cache.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsAvailableAsync_DomainExists_ReturnsFalse()
    {
        var rdapResult = CreateTestDomainInfo("example.com", LookupProtocol.Rdap);
        _rdapClient.LookupAsync("example.com", Arg.Any<CancellationToken>())
            .Returns(rdapResult);

        using var client = new DomainLookupClient(_whoisClient, _rdapClient, _cache);
        var available = await client.IsAvailableAsync("example.com");

        available.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_DomainNotFound_ReturnsTrue()
    {
        _rdapClient.LookupAsync("notexist123.com", Arg.Any<CancellationToken>())
            .Throws(new DomainNotFoundException("notexist123.com"));

        _whoisClient.QueryAsync("notexist123.com", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Throws(new DomainNotFoundException("notexist123.com"));

        using var client = new DomainLookupClient(_whoisClient, _rdapClient, _cache);
        var available = await client.IsAvailableAsync("notexist123.com");

        available.Should().BeTrue();
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        var client = new DomainLookupClient(_whoisClient, _rdapClient, _cache);
        client.Dispose();
        var act = () => client.Dispose();
        act.Should().NotThrow();
    }

    private static DomainInfo CreateTestDomainInfo(string domain, LookupProtocol protocol = LookupProtocol.Whois) => new()
    {
        DomainName = domain,
        Tld = "com",
        Protocol = protocol,
    };
}
