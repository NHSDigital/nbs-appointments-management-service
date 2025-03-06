using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Features;

public interface IFunctionFeatureToggleHelper
{
    Task<bool> IsFeatureEnabled(string featureFlag, FunctionContext functionContext,
        ClaimsPrincipal principal, IRequestInspector requestInspector);
}
