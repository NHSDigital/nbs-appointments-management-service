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

namespace Nhs.Appointments.Api.Integration.Scenarios.AttributeDefinitions;

[FeatureFile("./Scenarios/AttributeDefinitions/AttributeDefinitions.feature")]

public sealed class AttributeDefinitionsFeatureSteps : BaseFeatureSteps
{
    private  HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private IEnumerable<AttributeDefinition> _actualResponse;
    
    [Given("There are existing system attributes")]
    public async Task SetUpAttributes(Gherkin.Ast.DataTable dataTable)
    {
        var attributeDefinitions = dataTable.Rows.Skip(1).Select(
            row => new AttributeDefinition(row.Cells.ElementAt(0).Value, row.Cells.ElementAt(1).Value));
        
        var attributeDocument = new AttributeDefinitionsDocument()
            {
                Id = "attributes",
                DocumentType = "system",
                AttributeDefinitions = attributeDefinitions
            };
        
        await Client.GetContainer("appts", "index_data").UpsertItemAsync(attributeDocument);
    }
    
    [When(@"I query for all attribute definitions")]
    public async Task QueryForAttributeDefinitions()
    {
        _response = await Http.GetAsync("http://localhost:7071/api/attributeDefinitions");
        _statusCode = _response.StatusCode;
        _actualResponse = await JsonRequestReader.ReadRequestAsync<IEnumerable<AttributeDefinition>>(await _response.Content.ReadAsStreamAsync());
    }
    
    [Then("the following attribute definitions are returned")] 
    public async Task Assert(Gherkin.Ast.DataTable dataTable)
    {
        _statusCode.Should().Be(HttpStatusCode.OK);
        var expectedAttributeDefinitions = dataTable.Rows.Skip(1).Select(
            row => new AttributeDefinition(row.Cells.ElementAt(0).Value, row.Cells.ElementAt(1).Value));
        _actualResponse.Should().BeEquivalentTo(expectedAttributeDefinitions);
    }
}
