using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Core.Availability;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;

public abstract class AvailabilitySummaryFeatureSteps : BaseFeatureSteps
{
    protected AvailabilitySummary _actualResponse;
    protected HttpResponseMessage _response;
    protected HttpStatusCode _statusCode;

    [And("the following session summaries on day '(.+)' are returned")]
    [Then("the following session summaries on day '(.+)' are returned")]
    public void AssertSessionSummaries(string date, DataTable expectedSessionSummaryTable)
    {
        var expectedSessionSummaries = expectedSessionSummaryTable.Rows.Skip(1).Select(row =>
            new SessionAvailabilitySummary
            {
                UkStartDatetime =
                    NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value)
                        .ToDateTime(TimeOnly.Parse(row.Cells.ElementAt(1).Value)),
                UkEndDatetime = NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value)
                    .ToDateTime(TimeOnly.Parse(row.Cells.ElementAt(2).Value)),
                TotalSupportedAppointmentsByService = GetServiceBookings(row.Cells.ElementAt(3).Value),
                Capacity = int.Parse(row.Cells.ElementAt(4).Value),
                SlotLength = int.Parse(row.Cells.ElementAt(5).Value),
                MaximumCapacity = int.Parse(row.Cells.ElementAt(6).Value)
            });

        var expectedDate = NaturalLanguageDate.Parse(date);

        _statusCode.Should().Be(HttpStatusCode.OK);
        _actualResponse.DaySummaries.Single(x => x.Date == expectedDate).SessionSummaries.Should().BeEquivalentTo(
            expectedSessionSummaries,
            //exclude ID as randomly generated for linking purposes
            options => options.Excluding(x => x.Id));
    }

    [Then("the following day summary metrics are returned")]
    [And("the following day summary metrics are returned")]
    public void AssertDaySummaryMetrics(DataTable expectedDaySummaryTable)
    {
        // Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
        var expectedDaySummaries = expectedDaySummaryTable.Rows.Skip(1).Select(row =>
             (
                 NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value),
                int.Parse(row.Cells.ElementAt(1).Value),
                int.Parse(row.Cells.ElementAt(2).Value),
                int.Parse(row.Cells.ElementAt(3).Value),
                int.Parse(row.Cells.ElementAt(4).Value),
                int.Parse(row.Cells.ElementAt(5).Value)
            )).ToList();

        _statusCode.Should().Be(HttpStatusCode.OK);
        
        _actualResponse.DaySummaries.Count().Should().Be(expectedDaySummaries.Count);

        for (var i = 0; i < _actualResponse.DaySummaries.Count(); i++)
        {
            var actualDaySummary = _actualResponse.DaySummaries.ElementAt(i);
            var expectedDaySummary = expectedDaySummaries.ElementAt(i);

            actualDaySummary.Date.Should().Be(expectedDaySummary.Item1);
            actualDaySummary.MaximumCapacity.Should().Be(expectedDaySummary.Item2);
            actualDaySummary.TotalRemainingCapacity.Should().Be(expectedDaySummary.Item3);
            actualDaySummary.TotalSupportedAppointments.Should().Be(expectedDaySummary.Item4);
            actualDaySummary.TotalOrphanedAppointments.Should().Be(expectedDaySummary.Item5);
            actualDaySummary.TotalCancelledAppointments.Should().Be(expectedDaySummary.Item6);
        }
    }

    [Then("the following week summary metrics are returned")]
    [And("the following week summary metrics are returned")]
    public void AssertWeekSummaryMetrics(DataTable expectedWeekSummaryTable)
    {
        var row = expectedWeekSummaryTable.Rows.Last();

        // | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments |
        _actualResponse.MaximumCapacity.Should().Be(int.Parse(row.Cells.ElementAt(0).Value));
        _actualResponse.TotalRemainingCapacity.Should().Be(int.Parse(row.Cells.ElementAt(1).Value));
        _actualResponse.TotalSupportedAppointments.Should().Be(int.Parse(row.Cells.ElementAt(2).Value));
        _actualResponse.TotalOrphanedAppointments.Should().Be(int.Parse(row.Cells.ElementAt(3).Value));

        _statusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Then(@"a bad request error is returned")]
    public void Assert()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static Dictionary<string, int> GetServiceBookings(string cellValue)
    {
        var serviceBookings = cellValue.Split(',');
        return serviceBookings.Select(serviceBooking => serviceBooking.Trim().Split(':'))
            .ToDictionary(parts => parts[0], parts => int.Parse(parts[1]));
    }
}
