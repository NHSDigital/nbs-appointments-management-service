using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Nhs.Appointments.Api.Availability;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability
{
    [FeatureFile("./Scenarios/Availability/AvailabilityByDays.feature")]
    public sealed class AvailabilityByDaysFeatureSteps : AvailabilityBaseFeatureSteps
    {
        [Then(@"the following daily availability is returned")]
        public async Task AssertDailyAvailability(Gherkin.Ast.DataTable expectedDailyAvailabilityTable)
        {
            var expectedAvailabilty = expectedDailyAvailabilityTable.Rows.Skip(1).Select(row => new QueryAvailabilityResponseInfo
            (
                DateOnly.ParseExact(row.Cells.ElementAt(0).Value, "yyyy-MM-dd"),
                new List<QueryAvailabilityResponseBlock>()
                {
                    new (new TimeOnly(0,0), new TimeOnly(11,59), int.Parse(row.Cells.ElementAt(1).Value)),
                    new (new TimeOnly(12,0), new TimeOnly(23,59), int.Parse(row.Cells.ElementAt(2).Value)),
                }
            ));

            StatusCode.Should().Be(HttpStatusCode.OK);
            ActualReponse
                .First().availability
                .Should().BeEquivalentTo(expectedAvailabilty);
        }

        [Then(@"the request is successful and no availability is returned")]
        public async Task AssertNoAvailability()
        {
            StatusCode.Should().Be(HttpStatusCode.OK);
            ActualReponse.Should().BeEmpty();

        }
    }
}
