using DevWorker.WhoisNET.Cache;
using DevWorker.WhoisNET.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DevWorker.WhoisNET.Tests.Unit;

public class MemoryLookupCacheTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly MemoryLookupCache _cache;

    public MemoryLookupCacheTests()
    {
        _cache = new MemoryLookupCache(_memoryCache);
    }

    [Fact]
    public async Task GetAsync_Miss_ReturnsNull()
    {
        var result = await _cache.GetAsync("nonexistent.com");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAndGet_ReturnsStoredValue()
    {
        var info = CreateTestDomainInfo("example.com");
        await _cache.SetAsync("example.com", info);

        var result = await _cache.GetAsync("example.com");
        result.Should().NotBeNull();
        result!.DomainName.Should().Be("example.com");
    }

    [Fact]
    public async Task SetAndGet_NormalizesDomain()
    {
        var info = CreateTestDomainInfo("example.com");
        await _cache.SetAsync("EXAMPLE.COM", info);

        var result = await _cache.GetAsync("example.com");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveAsync_RemovesCachedEntry()
    {
        var info = CreateTestDomainInfo("example.com");
        await _cache.SetAsync("example.com", info);
        await _cache.RemoveAsync("example.com");

        var result = await _cache.GetAsync("example.com");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithShortDuration_Expires()
    {
        var info = CreateTestDomainInfo("example.com");
        await _cache.SetAsync("example.com", info, TimeSpan.FromMilliseconds(50));

        await Task.Delay(100);

        var result = await _cache.GetAsync("example.com");
        result.Should().BeNull();
    }

    private static DomainInfo CreateTestDomainInfo(string domain) => new()
    {
        DomainName = domain,
        Tld = "com",
        Protocol = LookupProtocol.Whois,
    };

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}
