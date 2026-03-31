using DevWorker.WhoisNET.Internals;

namespace DevWorker.WhoisNET.Tests.Unit;

public class DateParserTests
{
    [Theory]
    [InlineData("2024-01-15T10:00:00Z")]
    [InlineData("2024-01-15T10:00:00.0Z")]
    [InlineData("2024-01-15T10:00:00+00:00")]
    [InlineData("2024-01-15 10:00:00")]
    [InlineData("2024-01-15")]
    public void TryParse_Iso8601_ReturnsDate(string input)
    {
        var result = DateParser.TryParse(input);
        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(2024);
        result.Value.Month.Should().Be(1);
        result.Value.Day.Should().Be(15);
    }

    [Theory]
    [InlineData("15-Jan-2025")]
    [InlineData("15-Jan-2025 10:00:00")]
    public void TryParse_UkFormat_ReturnsDate(string input)
    {
        var result = DateParser.TryParse(input);
        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(2025);
        result.Value.Month.Should().Be(1);
        result.Value.Day.Should().Be(15);
    }

    [Theory]
    [InlineData("1997-09-15T04:00:00Z", 1997, 9, 15)]
    [InlineData("2028-09-14T04:00:00Z", 2028, 9, 14)]
    public void TryParse_SpecificDates_ReturnsExactDate(string input, int year, int month, int day)
    {
        var result = DateParser.TryParse(input);
        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(year);
        result.Value.Month.Should().Be(month);
        result.Value.Day.Should().Be(day);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a date")]
    [InlineData("abc-def-ghi")]
    public void TryParse_InvalidInput_ReturnsNull(string? input)
    {
        var result = DateParser.TryParse(input);
        result.Should().BeNull();
    }
}
