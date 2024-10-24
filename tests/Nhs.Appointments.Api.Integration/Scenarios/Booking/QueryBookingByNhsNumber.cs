﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;


namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/QueryBookingByNhsNumber.feature")]
    public sealed class QueryBookingByNhsNumber : BaseFeatureSteps
    {
        private  HttpResponseMessage _response;
        private HttpStatusCode _statusCode;
        private List<Core.Booking> _actualResponse;
        
        [When(@"I query for bookings for a person using their NHS number")]
        public async Task CheckAvailability()
        {
            _response = await Http.GetAsync($"http://localhost:7071/api/booking?nhsNumber={NhsNumber}");
            _statusCode = _response.StatusCode;
            _actualResponse = await JsonRequestReader.ReadRequestAsync<List<Core.Booking>>(await _response.Content.ReadAsStreamAsync());
        }
        
        [Then(@"the following bookings are returned")]
        public async Task Assert(Gherkin.Ast.DataTable expectedBookingDetailsTable)
        {;
            var expectedBookings = expectedBookingDetailsTable.Rows.Skip(1).Select(
                (row, index) =>
                new Core.Booking()
                {
                    Reference = GetBookingReference(index.ToString()),
                    From = DateTime.ParseExact($"{row.Cells.ElementAt(0).Value} {row.Cells.ElementAt(1).Value}", "yyyy-MM-dd HH:mm", null),
                    Duration = int.Parse(row.Cells.ElementAt(2).Value),
                    Service = row.Cells.ElementAt(3).Value,
                    Site = GetSiteId(),
                    AttendeeDetails = new AttendeeDetails
                    {
                        NhsNumber = NhsNumber,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        DateOfBirth = new DateOnly(2000, 1, 1)
                    }
                }).ToList();

            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.Should().BeEquivalentTo(expectedBookings);
        }
        
        [Then(@"the request is successful and no bookings are returned")]
        public async Task AssertNoAvailability()
        {
            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.Should().BeEmpty();
        }
    }
}
