using System;
using System.Text.RegularExpressions;

namespace Nhs.Appointments.Api.Integration.Data;

public static partial class NaturalLanguageDate
{
    [GeneratedRegex(
        "^(?<format>Today|today|Tomorrow|tomorrow|Yesterday|yesterday|Next Monday|Next Tuesday|Next Wednesday|Next Thursday|Next Friday|Next Saturday|Next Sunday|(((?<magnitude>[0-9]+) (?<period>days|day|weeks|week|months|month|years|year) (?<direction>from|before) (now|today))))$")]
    private static partial Regex NaturalLanguageRelativeDate();

    /// <summary>
    ///     Parses natural language dates like "Tomorrow" or "Next Tuesday" into DateOnly objects relative to the current date.
    /// </summary>
    /// <param name="dateString"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static DateOnly Parse(string dateString)
    {
        // First, check if it matches a "Today [+/- N]" pattern
        var todayPlusNMatch = Regex.Match(dateString, @"^Today(\s*([+-]\d+))?$", RegexOptions.IgnoreCase);
        if (todayPlusNMatch.Success)
        {
            int.TryParse(todayPlusNMatch.Groups[2].Value, out var dayOffset);

            return DateOnly.FromDateTime(DateTime.UtcNow).AddDays(dayOffset);
        }

        var match = NaturalLanguageRelativeDate().Match(dateString);
        if (!match.Success)
        {
            throw new FormatException("Date string not recognised.");
        }

        var format = match.Groups["format"].Value;
        switch (format)
        {
            case "Tomorrow":
            case "tomorrow":
                return DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            case "Yesterday":
            case "yesterday":
                return DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            case "Today":
            case "today":
                return DateOnly.FromDateTime(DateTime.UtcNow);
            case "Next Monday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Monday));
            case "Next Tuesday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Tuesday));
            case "Next Wednesday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Wednesday));
            case "Next Thursday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Thursday));
            case "Next Friday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Friday));
            case "Next Saturday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Saturday));
            case "Next Sunday":
                return DateOnly.FromDateTime(GetDayInNextWeek(DayOfWeek.Sunday));
        }

        var period = match.Groups["period"].Value;
        var direction = match.Groups["direction"].Value;
        var magnitude = match.Groups["magnitude"].Value;

        var offset = direction == "from" ? int.Parse(magnitude) : int.Parse(magnitude) * -1;
        return period switch
        {
            "days" => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset),
            "day" => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset),
            "weeks" => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset * 7),
            "week" => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset * 7),
            "months" => DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(offset),
            "month" => DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(offset),
            "years" => DateOnly.FromDateTime(DateTime.UtcNow).AddYears(offset),
            "year" => DateOnly.FromDateTime(DateTime.UtcNow).AddYears(offset),
            _ => throw new FormatException("Error parsing natural language date regex")
        };
    }

    /// <summary>
    ///     Want to return a day of the week in the next week.
    /// </summary>
    /// <param name="targetDay"></param>
    /// <returns></returns>
    private static DateTime GetDayInNextWeek(DayOfWeek targetDay)
    {
        var today = DateTime.UtcNow;

        // Get this week's Monday
        var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var thisWeeksMonday = today.AddDays(-daysSinceMonday);

        // Get next week's Monday
        var nextWeeksMonday = thisWeeksMonday.AddDays(7);

        // Calculate days to target day from next Monday
        var daysToTarget = ((int)targetDay - (int)DayOfWeek.Monday + 7) % 7;

        return nextWeeksMonday.AddDays(daysToTarget);
    }
}
