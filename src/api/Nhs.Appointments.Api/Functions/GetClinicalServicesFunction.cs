using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions
{
    public class GetClinicalServicesFunction(
        IValidator<EmptyRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<GetClinicalServicesFunction> logger,
        IMetricsRecorder metricsRecorder,
        IClinicalService clinicalService,
        IFeatureToggleHelper featureToggleHelper)
        : BaseApiFunction<EmptyRequest, IEnumerable<ClinicalServiceType>>(validator, userContextProvider, logger, metricsRecorder)
    {
        [OpenApiOperation(operationId: "GetServiceTypes", tags: ["serviceTypes"], 
            Summary = "Get Services Types for Availabilities Form")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<ClinicalServiceType>), 
            Description = "List of service types available")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",typeof(ErrorMessageResponseItem), 
            Description = "Unauthorized request to a protected API")]
        [Function("GetClinicalServicesFunction")]
        public override async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "clinical-services")]
            HttpRequest req)
        {
            return !await featureToggleHelper.IsFeatureEnabled(Flags.BulkImport)
                ? ProblemResponse(HttpStatusCode.NotImplemented, null)
                : await base.RunAsync(req);
        }

        protected override async Task<ApiResult<IEnumerable<ClinicalServiceType>>> HandleRequest(EmptyRequest request, ILogger logger)
        {
            var serviceTypes = await clinicalService.Get();
            return ApiResult<IEnumerable<ClinicalServiceType>>.Success(serviceTypes);
        }


        protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
        {
            return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
        }
    }
}
