using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions
{
    public class GetEulaFunction(
        IEulaService eulaService,
        IValidator<EmptyRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<GetEulaFunction> logger,
        IMetricsRecorder metricsRecorder)
        : BaseApiFunction<EmptyRequest, EulaVersion>(validator, userContextProvider, logger, metricsRecorder)
    {
        [OpenApiOperation(operationId: "GetEula", tags: ["Eula"], Summary = "Gets the End-User Licence Agreement")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(EulaVersion),
            Description = "The latest version of the End-User Licence Agreement")]
        [Function("GetEulaFunction")]
        public override Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eula")]
            HttpRequest req, FunctionContext functionContext)
        {
            return base.RunAsync(req, functionContext);
        }

        protected override async Task<ApiResult<EulaVersion>> HandleRequest(EmptyRequest request, ILogger logger,
            FunctionContext functionContext)
        {
            var eula = await eulaService.GetEulaVersionAsync();

            return ApiResult<EulaVersion>.Success(eula);
        }

        protected override Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(EmptyRequest request)
        {
            return Task.FromResult(Enumerable.Empty<ErrorMessageResponseItem>());
        }
    }
}
