using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

public abstract class SiteManagementBaseFeatureSteps : BaseFeatureSteps
{
    private ErrorMessageResponseItem? ErrorResponse { get; set; }
    protected HttpResponseMessage? Response { get; set; }
    protected Site? ActualResponse { get; set; }

    [Given("The site '(.+)' does not exist in the system")]
    public Task NoSite()
    {
        return Task.CompletedTask;
    }
    
    [Given("The following sites exist in the system")]
    public async Task SetUpSites(Gherkin.Ast.DataTable dataTable)
    {
        var sites = dataTable.Rows.Skip(1).Select(
            row => new SiteDocument()
            {
                Id = GetSiteId(row.Cells.ElementAt(0).Value),
                Name = row.Cells.ElementAt(1).Value,
                Address = row.Cells.ElementAt(2).Value,
                PhoneNumber = row.Cells.ElementAt(3).Value,
                DocumentType = "site",
                AttributeValues = ParseAttributes(row.Cells.ElementAt(4).Value),
                Location = new Location("Point", new[] { double.Parse(row.Cells.ElementAt(5).Value), double.Parse(row.Cells.ElementAt(6).Value) }),
            });
        
        foreach (var site in sites)
        {
            await Client.GetContainer("appts", "index_data").UpsertItemAsync(site);
        }
    }
    
    [Then("a message is returned saying the site is not found")]
    public async Task Assert()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (_, ErrorResponse) = await JsonRequestReader.ReadRequestAsync<ErrorMessageResponseItem>(await Response.Content.ReadAsStreamAsync());
        ErrorResponse.Message.Should().Be("The specified site was not found.");
    }
    
    protected static AttributeValue[] ParseAttributes(string attributes)
    {
        if (attributes == "__empty__")
        {
            return Array.Empty<AttributeValue>();
        }
        var pairs = attributes.Split(",");
        return pairs.Select(p => p.Trim().Split("=")).Select(kvp => new AttributeValue(kvp[0], kvp[1])).ToArray();
    }
}
