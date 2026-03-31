# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2026-03-31

### Added
- Initial release
- RDAP client with IANA bootstrap resolution
- WHOIS TCP client with referral handling
- Specialized parsers for .com, .net, .org, .fr, .de, .uk, .eu, .io
- Generic key-value WHOIS parser for all other TLDs
- In-memory caching with configurable TTL
- DI integration via `IServiceCollection.AddWhoisNet()`
- Standalone usage via `new DomainLookupClient()`
- Domain availability check (`IsAvailableAsync`)
- Raw WHOIS/RDAP response access
- Multi-targeting: netstandard2.0, net6.0, net8.0, net10.0
- Rate limiting detection
- WHOIS server database with 40+ TLDs
- IANA fallback for unknown TLDs
- Multi-format date parser
- Comprehensive exception hierarchy
