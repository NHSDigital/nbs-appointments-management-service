using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability;

public abstract class CreateAvailabilityFeatureSteps : BaseFeatureSteps
{
    private readonly List<AvailabilityCreatedEvent> _expectedAvailabilityCreatedEvents = [];

    [And(@"the following availability created event is created")]
    [Then(@"the following availability created event is created")]
    public async Task AssertAvailabilityCreatedEventsAsync(DataTable dataTable)
    {
        PopulateExpectedAvailabilityCreatedEventsFromTable(dataTable);

        var actualAvailabilityCreatedEvents = await GetActualAvailabilityCreatedEvents();

        actualAvailabilityCreatedEvents.Should().BeEquivalentTo(_expectedAvailabilityCreatedEvents,
            options => options.Excluding(
                x => x.Created));
    }

    private void PopulateExpectedAvailabilityCreatedEventsFromTable(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows.Skip(1))
        {
            if (row.Location.Line == 0)
            {
                continue;
            }

            var cells = row.Cells.ToList();
            var type = cells.ElementAt(0).Value;
            var by = cells.ElementAt(1).Value;
            var fromDate = DeriveRelativeDateOnly(cells.ElementAt(2).Value);
            var toDate = DeriveRelativeDateOnly(cells.ElementAt(3).Value);
            var templateDays = DeriveWeekDaysInRange(fromDate, toDate);
            var fromTime = cells.ElementAt(5).Value;
            var untilTime = cells.ElementAt(6).Value;
            var slotLength = cells.ElementAt(7).Value;
            var capacity = cells.ElementAt(8).Value;
            var services = cells.ElementAt(9).Value;

            var session = new Session()
            {
                From = TimeOnly.Parse(fromTime),
                Until = TimeOnly.Parse(untilTime),
                SlotLength = int.Parse(slotLength),
                Capacity = int.Parse(capacity),
                Services = services.Split(',').Select(s => s.Trim()).ToArray()
            };

            var template = type == "Template"
                ? new Template()
                {
                    Days = ParseDays(templateDays),
                    Sessions = [session]
                } : null;

            var expectedEvent = new AvailabilityCreatedEvent()
            {
                Created = DateTime.UtcNow,
                By = by,
                Site = GetSiteId(),
                From = fromDate,
                To = type == "Template" ? toDate : null,
                Template = template,
                Sessions = type == "SingleDateSession" ? [session] : null
            };

            _expectedAvailabilityCreatedEvents.Add(expectedEvent);
        }
    }

    private async Task<List<AvailabilityCreatedEventDocument>> GetActualAvailabilityCreatedEvents()
    {
        var siteId = GetSiteId();

        var container = Client.GetContainer("appts", "booking_data");
        var actualDocuments =
            await RunQueryAsync<AvailabilityCreatedEventDocument>(
                container,
                d => d.DocumentType == "availability_created_event"
                     && d.Site == siteId);
        return actualDocuments.ToList();
    }
}
