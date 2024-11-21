using System;
using FluentAssertions;
using Nhs.Appointments.Api.Integration.Scenarios;
using Xunit;

namespace Nhs.Appointments.Api.Integration;

public class BaseFeatureStepsTests
{
    [Theory]
    [InlineData("Today", 0)]
    [InlineData("Tomorrow", 1)]
    [InlineData("Yesterday", -1)]
    [InlineData("3 days from now", 3)]
    [InlineData("3 days from today", 3)]
    [InlineData("3 days before now", -3)]
    [InlineData("1 day before now", -1)]
    [InlineData("1 week from now", 7)]
    [InlineData("2 weeks from today", 14)]
    [InlineData("13 weeks before now", -91)]
    public void CanParseNaturalLanguageDates(string input, int expectedDaysFromToday)
    {
        var expectedDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(expectedDaysFromToday);
        var actualDate = BaseFeatureSteps.ParseNaturalLanguageDateOnly(input);

        actualDate.Should().Be(expectedDate);
    }

    [Fact]
    public void CanParseNaturalLanguageDates_WithMonths()
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = BaseFeatureSteps.ParseNaturalLanguageDateOnly("1 month from today");

        if (result.Year == now.Year)
        {
            result.Month.Should().Be(now.Month + 1);
        }
        else
        {
            // Next month was 1st month of next year
            result.Month.Should().Be(1);
        }
    }

    [Fact]
    public void CanParseNaturalLanguageDates_WithYears()
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = BaseFeatureSteps.ParseNaturalLanguageDateOnly("1 year from today");

        result.Year.Should().Be(now.Year + 1);
        result.Month.Should().Be(now.Month);

        var is29thOfFeb = $"{now:MM-dd}" == "02-29";
        if (is29thOfFeb)
        {
            result.Day.Should().Be(28);
        }
        else
        {
            result.Day.Should().Be(now.Day);
        }
    }
}