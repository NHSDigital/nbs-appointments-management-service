using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core.Bookings;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/QueryBookingByNhsNumber.feature")]
    public class QueryBookingByNhsNumber : BookingBaseFeatureSteps
    {
        private List<Core.Bookings.Booking> _actualResponse;
        private HttpStatusCode _statusCode;

        [When(@"I query for bookings for a person using their NHS number")]
        public async Task CheckAvailability()
        {
            _response = await Http.GetAsync($"http://localhost:7071/api/booking?nhsNumber={NhsNumber}");
            _statusCode = _response.StatusCode;
            (_, _actualResponse) =
                await JsonRequestReader.ReadRequestAsync<List<Core.Bookings.Booking>>(
                    await _response.Content.ReadAsStreamAsync());
        }

        [Then(@"the request is successful and no bookings are returned")]
        public void AssertNoAvailability()
        {
            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.Should().BeEmpty();
        }
    }
}
