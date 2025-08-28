using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.WellKnownOdsCodeEntries;

[FeatureFile("./Scenarios/WellKnownOdsCodeEntries/WellKnownOdsCodeEntries.feature")]

public sealed class WellKnownOdsCodeEntriesFeatureSteps : BaseFeatureSteps
{
    private  HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private IEnumerable<WellKnownOdsEntry> _actualResponse;
    
    [Given("there are existing well known ods codes documented")]
    public async Task SetUpWellKnownOdsCodesDocument()
    {
        var wellKnownOdsCodesDocument = new WellKnownOdsCodesDocument()
            {
                Id = "well_known_ods_codes",
                DocumentType = "system",
                Entries = [
                new WellKnownOdsEntry(
                    OdsCode: "R1",
                    DisplayName: "Region One",
                    Type: "region"),
                new WellKnownOdsEntry(
                    OdsCode: "ICB1",
                    DisplayName: "Integrated Care Board One",
                    Type: "icb")
                ]
            };
        
        await Client.GetContainer("appts", "core_data").UpsertItemAsync(wellKnownOdsCodesDocument);
    }
    
    [When(@"I query for well known ods code entries")]
    public async Task QueryForWellKnownOdsCodeEntries()
    {
        _response = await Http.GetAsync("http://localhost:7071/api/wellKnownOdsCodeEntries");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<IEnumerable<WellKnownOdsEntry>>(await _response.Content.ReadAsStreamAsync());
    }
    
    [Then("the following entries are returned")] 
    public void Assert(Gherkin.Ast.DataTable dataTable)
    {
        _statusCode.Should().Be(HttpStatusCode.OK);
        var expectedWellKnownOdsCodeEntries = dataTable.Rows.Skip(1).Select(
            row => new WellKnownOdsEntry(row.Cells.ElementAt(0).Value, row.Cells.ElementAt(1).Value, row.Cells.ElementAt(2).Value));
        _actualResponse.Should().BeEquivalentTo(expectedWellKnownOdsCodeEntries);
    }
}
