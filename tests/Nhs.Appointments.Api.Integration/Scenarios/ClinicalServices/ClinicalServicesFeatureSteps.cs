using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.ClinicalServices
{
    [FeatureFile("./Scenarios/ClinicalServices/ClinicalServices.feature")]
    public class ClinicalServicesBaseFeatureSteps : BaseFeatureSteps
    {
        private HttpResponseMessage _response;
        private HttpStatusCode _statusCode;
        private IEnumerable<ClinicalServiceType> _actualResponse;

        [When(@"I request Clinical Services")]
        public async Task RequestClinicalServices() 
        {
            _response = await Http.GetAsync("http://localhost:7071/api/clinical-services");

            _statusCode = _response.StatusCode;
            (_, _actualResponse) =
                await JsonRequestReader.ReadRequestAsync<IEnumerable<ClinicalServiceType>>(
                    await _response.Content.ReadAsStreamAsync());
        }

        [Given("I have Clinical Services")]
        [And("I have Clinical Services")]
        public async Task SetUpClinicalServices(DataTable dataTable) 
        {
            var clinicalServices = dataTable.Rows.Skip(1).Select(row => new ClinicalServiceTypeDocument
            {
                Id = dataTable.GetRowValueOrDefault(row, "Id"),
                Label = dataTable.GetRowValueOrDefault(row, "Label"),
                ServiceType = dataTable.GetRowValueOrDefault(row, "Type"),
                Url = dataTable.GetRowValueOrDefault(row, "Url")
            });

            var clinicalServicesDocument = new ClinicalServiceDocument()
            {
                Id = "clinical_services",
                DocumentType = "system",
                Services = clinicalServices.ToArray()
            };

            await Client.GetContainer("appts", "core_data").UpsertItemAsync(clinicalServicesDocument);
        }

        [Then("the request should return Clinical Services")]
        public async Task AssertServicesReturns(DataTable dataTable) 
        {
            var clinicalServices = dataTable.Rows.Skip(1).Select(x => new ClinicalServiceType()
            {
                Value = x.Cells.ElementAt(0).Value
            });

            _actualResponse.Should().BeEquivalentTo(clinicalServices);
        }

        [Then(@"the request should be successful")]
        public async Task AssertHttpOk() => _statusCode.Should().Be(HttpStatusCode.OK);

        [Then(@"the request should be Not Implemented")]
        public async Task AssertHttpNotImplemented() => _statusCode.Should().Be(HttpStatusCode.NotImplemented);
    }
}
