using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Cancel;

public abstract class CancelBookingBaseFeatureSteps : BookingBaseFeatureSteps
{
    [When("I cancel the following bookings")]
    public async Task CancelBookings(DataTable dataTable)
    {
        var (url, payload) = BuildCancelBookingPayload(dataTable);
    
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    
        _response = await GetHttpClientForTest().PostAsync(url, content);
    }
    
    [Then(@"a bad request error is returned")]
    public void Assert()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    private (string url, object payload) BuildCancelBookingPayload(DataTable table)
    {
        var row = table.Rows.ElementAt(1);
    
        var bookingReference = CreateUniqueTestValue(table.GetRowValueOrDefault(row, "Reference")) ??
                               BookingReferences.GetBookingReference(0, BookingType.Confirmed);
        var url = $"http://localhost:7071/api/booking/{bookingReference}/cancel";
    
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
    
        var site = table.GetRowValueOrDefault(row, "Site");
        if (site != null)
        {
            return ($"{url}?site={GetSiteId(site)}", payload);
        }
    
        return (url, payload);
    }
}
