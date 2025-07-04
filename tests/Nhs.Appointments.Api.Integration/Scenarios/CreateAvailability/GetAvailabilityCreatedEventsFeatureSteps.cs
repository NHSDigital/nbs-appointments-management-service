﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/GetAvailabilityCreatedEvents.feature")]
    public abstract class GetAvailabilityCreatedEventsFeatureSteps(string flag, bool enabled)
        : BaseCreateAvailabilityFeatureSteps(flag, enabled)
    {
        private IEnumerable<AvailabilityCreatedEvent> _actualResponse;

        [Then("I request Availability Created Events for the current site")]
        public async Task ThenIRequestAvailabilityCreatedEventsForTheCurrentSite()
        {
            var siteId = GetSiteId();
            var dateFrom = ParseNaturalLanguageDateOnly("Yesterday");

            _response = await Http.GetAsync(
                $"http://localhost:7071/api/availability-created?site={siteId}&from={dateFrom:yyyy-MM-dd}");
            _statusCode = _response.StatusCode;
            var content = await _response.Content.ReadAsStreamAsync();

            (_, _actualResponse) =
                await JsonRequestReader.ReadRequestAsync<IEnumerable<AvailabilityCreatedEvent>>(content);

            _statusCode.Should().Be(HttpStatusCode.OK);

            _actualResponse.Should().BeEquivalentTo(_expectedAvailabilityCreatedEvents,
                options => options.Excluding(x => x.Created));
        }
    }

    [Collection("MultipleServicesSerialToggle")]
    public class GetAvailabilityCreatedEventsFeatureSteps_MultipleServicesEnabled()
        : GetAvailabilityCreatedEventsFeatureSteps(Flags.MultipleServices, true);

    [Collection("MultipleServicesSerialToggle")]
    public class GetAvailabilityCreatedEventsFeatureSteps_MultipleServicesDisabled()
        : GetAvailabilityCreatedEventsFeatureSteps(Flags.MultipleServices, false);
}
