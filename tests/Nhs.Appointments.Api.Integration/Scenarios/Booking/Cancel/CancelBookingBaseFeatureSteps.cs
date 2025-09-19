﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Cancel;

public abstract class CancelBookingBaseFeatureSteps : BookingBaseFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _responseCode;
    private string _responseBody;

    [When("I cancel the following bookings")]
    public async Task ConfirmBookingsTwo(DataTable dataTable)
    {
        var (url, payload) = BuildCancelBookingPayload(dataTable);

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        _response = await Http.PostAsync(url, content);
        _responseCode = _response.StatusCode;
        _responseBody = await _response.Content.ReadAsStringAsync();
    }

    private (string url, object payload) BuildCancelBookingPayload(DataTable table)
    {
        var row = table.Rows.ElementAt(1);

        var bookingReference = CreateCustomBookingReference(table.GetRowValueOrDefault(row, "Reference")) ??
                               BookingReferences.GetBookingReference(0, BookingType.Confirmed);
        var site = GetSiteId();

        var payload = new Dictionary<string, object>();

        var cancellationReason = table.GetRowValueOrDefault(row, "Cancellation reason");
        if (cancellationReason != null)
        {
            payload["cancellationReason"] = cancellationReason;
        }

        var additionalData = BuildAdditionalDataFromDataTable(table, row);
        if (additionalData != null)
        {
            payload["additionalData"] = additionalData;
        }

        return ($"http://localhost:7071/api/booking/{bookingReference}/cancel?site={site}", payload);
    }
}
