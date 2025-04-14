using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions
{
    public class GetServiceTypesFunction(
        IValidator<EmptyRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<GetServiceTypesFunction> logger,
        IMetricsRecorder metricsRecorder,
        IServiceTypeService serviceTypeService)
        : BaseApiFunction<EmptyRequest, IEnumerable<ServiceType>>(validator, userContextProvider, logger, metricsRecorder)
    {
        [OpenApiOperation(operationId: "GetServiceTypes", tags: ["serviceTypes"], 
            Summary = "Get Services Types for Availibilities Form")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<ServiceType>), 
            Description = "List of service types available")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json",typeof(ErrorMessageResponseItem), 
            Description = "Unauthorized request to a protected API")]
        [Function("GetServiceTypesFunction")]
        public override Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "serviceTypes")]
            HttpRequest req)
        {
            return base.RunAsync(req);
        }

        protected override async Task<ApiResult<IEnumerable<ServiceType>>> HandleRequest(EmptyRequest request, ILogger logger)
        {
            var serviceTypes = await serviceTypeService.Get();
            return ApiResult<IEnumerable<ServiceType>>.Success(serviceTypes);

        }
    }
}
