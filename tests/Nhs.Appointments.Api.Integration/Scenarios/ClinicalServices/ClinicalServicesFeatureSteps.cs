using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core.ClinicalServices;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.ClinicalServices
{
    [FeatureFile("./Scenarios/ClinicalServices/ClinicalServices.feature")]
    public class ClinicalServicesBaseFeatureSteps : BaseFeatureSteps
    {
        private HttpStatusCode _statusCode;
        private IEnumerable<ClinicalServiceType> _actualResponse;

        [When(@"I request Clinical Services")]
        public async Task RequestClinicalServices() 
        {
            _response = await GetHttpClientForTest().GetAsync("http://localhost:7071/api/clinical-services");

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

            await CosmosAction_RetryOnTooManyRequests(CosmosAction.Upsert, Client.GetContainer("appts", "core_data"), clinicalServicesDocument);
        }

        [Then("the request should return Clinical Services")]
        public void AssertServicesReturns(DataTable dataTable) 
        {
            var clinicalServices = dataTable.Rows.Skip(1).Select(row => new ClinicalServiceType
            {
                Value = dataTable.GetRowValueOrDefault(row, "Id"),
                Label = dataTable.GetRowValueOrDefault(row, "Label"),
                ServiceType = dataTable.GetRowValueOrDefault(row, "Type"),
                Url = dataTable.GetRowValueOrDefault(row, "Url")
            });

            _actualResponse.Should().BeEquivalentTo(clinicalServices);
        }

        [Then("the request should return Clinical Services in exact order")]
        public void AssertServicesReturnsInOrder(DataTable dataTable)
        {
            var expectedIds = dataTable.Rows
                .Skip(1)
                .Select(row => dataTable.GetRowValueOrDefault(row, "Id"))
                .ToList();

            var actualIds = _actualResponse.Select(s => s.Value).ToList();

            actualIds.Should().Equal(expectedIds);
        }

        [Then(@"the request should be successful")]
        public void AssertHttpOk() => _statusCode.Should().Be(HttpStatusCode.OK);

        [Then(@"the request should be Not Implemented")]
        public void AssertHttpNotImplemented() => _statusCode.Should().Be(HttpStatusCode.NotImplemented);
    }
}
