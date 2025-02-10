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

namespace Nhs.Appointments.Api.Integration.Scenarios.AccessibilityDefinitions;

[FeatureFile("./Scenarios/AccessibilityDefinitions/AccessibilityDefinitions.feature")]

public sealed class AccessibilityDefinitionsFeatureSteps : BaseFeatureSteps
{
    private  HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private IEnumerable<AccessibilityDefinition> _actualResponse;
    
    [Given("There are existing system accessibilities")]
    public async Task SetUpAccessibilities(Gherkin.Ast.DataTable dataTable)
    {
        var AccessibilityDefinitions = dataTable.Rows.Skip(1).Select(
            row => new AccessibilityDefinition(row.Cells.ElementAt(0).Value, row.Cells.ElementAt(1).Value));
        
        var accessibilityDocument = new AccessibilityDefinitionsDocument()
            {
                Id = "accessibilities",
                DocumentType = "system",
                AccessibilityDefinitions = AccessibilityDefinitions
            };
        
        await Client.GetContainer("appts", "core_data").UpsertItemAsync(accessibilityDocument);
    }
    
    [When(@"I query for all accessibility definitions")]
    public async Task QueryForAccessibilityDefinitions()
    {
        _response = await Http.GetAsync("http://localhost:7071/api/AccessibilityDefinitions");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<IEnumerable<AccessibilityDefinition>>(await _response.Content.ReadAsStreamAsync());
    }
    
    [Then("the following accessibility definitions are returned")] 
    public void Assert(Gherkin.Ast.DataTable dataTable)
    {
        _statusCode.Should().Be(HttpStatusCode.OK);
        var expectedAccessibilityDefinitions = dataTable.Rows.Skip(1).Select(
            row => new AccessibilityDefinition(row.Cells.ElementAt(0).Value, row.Cells.ElementAt(1).Value));
        _actualResponse.Should().BeEquivalentTo(expectedAccessibilityDefinitions);
    }
}
