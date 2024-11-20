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
    [InlineData("1 month before now", -28)]
    [InlineData("13 weeks before now", -91)]
    [InlineData("1 year from today", 365)]
    public void CanParseNaturalLanguageDates(string input, int expectedDaysFromToday)
    {
        var expectedDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(expectedDaysFromToday);
        var actualDate = BaseFeatureSteps.ParseNaturalLanguageDateOnly(input);

        actualDate.Should().Be(expectedDate);
    }
}