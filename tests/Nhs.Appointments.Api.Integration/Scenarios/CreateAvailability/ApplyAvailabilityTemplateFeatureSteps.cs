using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/ApplyAvailabilityTemplate.feature")]
    public sealed class ApplyAvailabilityTemplateFeatureSteps : BaseCreateAvailabilityFeatureSteps
    {
    }
}
