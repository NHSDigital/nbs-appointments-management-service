using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;
using Nhs.Appointments.Api.Json;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Scenarios.ClinicalServices
{
    public abstract class ClinicalServicesBaseFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
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
            var clinicalServices = dataTable.Rows.Skip(1).Select(x => new ClinicalServiceTypeDocument()
            {
                Id = x.Cells.ElementAt(0).Value,
                Label = x.Cells.ElementAt(0).Value
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
                Value = x.Cells.ElementAt(0).Value,
                Label = x.Cells.ElementAt(0).Value
            });

            _actualResponse.Should().BeEquivalentTo(clinicalServices);
        }

        [Then(@"the request should be successful")]
        public async Task AssertHttpOk() => _statusCode.Should().Be(HttpStatusCode.OK);

        [Then(@"the request should be Not Implemented")]
        public async Task AssertHttpNotImplemented() => _statusCode.Should().Be(HttpStatusCode.NotImplemented);
    }
    
    [FeatureFile("./Scenarios/ClinicalServices/ClinicalServices_MultipleServicesEnabled.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class ClinicalServices_MultipleServicesEnabled()
        : ClinicalServicesBaseFeatureSteps(Flags.MultipleServices, true);

    [FeatureFile("./Scenarios/ClinicalServices/ClinicalServices_MultipleServicesDisabled.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class ClinicalServices_MultipleServicesDisabled()
        : ClinicalServicesBaseFeatureSteps(Flags.MultipleServices, false);
}
