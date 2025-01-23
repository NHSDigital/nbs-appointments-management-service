using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Functions;

    public class GetAvailabilityCreatedEventsFunction(IAvailabilityService availabilityService, IValidator<GetAvailabilityCreatedEventsRequest> validator, IUserContextProvider userContextProvider, ILogger<GetAvailabilityCreatedEventsFunction> logger, IMetricsRecorder metricsRecorder)
        : BaseApiFunction<GetAvailabilityCreatedEventsRequest, IEnumerable<AvailabilityCreatedEvent>>(validator, userContextProvider, logger, metricsRecorder)
    {
        [OpenApiOperation(operationId: "Get_AvailabilityCreatedEventsFunction", tags: ["Availability"], Summary = "Get records of availability created previously")]
        [OpenApiParameter("site", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The id of the site from which to retrieve previously created availability")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", typeof(IEnumerable<AvailabilityCreatedEvent>), Description = "List of previously created availability")]
        [OpenApiResponseWithBody(statusCode:HttpStatusCode.BadRequest, "application/json", typeof(IEnumerable<ErrorMessageResponseItem>),  Description = "The body of the request is invalid" )]
        [OpenApiResponseWithBody(statusCode:HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
        [OpenApiResponseWithBody(statusCode:HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
        [RequiresPermission("availability:setup", typeof(SiteFromQueryStringInspector))]
        [Function("GetAvailabilityCreatedEventsFunction")]
        public override Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "availability-created")] HttpRequest req)
        {
            return base.RunAsync(req);
        }

        protected override async Task<ApiResult<IEnumerable<AvailabilityCreatedEvent>>> HandleRequest(GetAvailabilityCreatedEventsRequest request, ILogger logger)
        {
            var availabilityCreatedEvents = await availabilityService.GetAvailabilityCreatedEventsAsync(request.Site, request.FromDate);

            return Success(availabilityCreatedEvents);
        }

        protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetAvailabilityCreatedEventsRequest request)>
            ReadRequestAsync(HttpRequest req)
        {
            var errors = new List<ErrorMessageResponseItem>();
            if (!req.Query.TryGetValue("site", out var site))
            {
                errors.Add(new ErrorMessageResponseItem { Message = "Error parsing site parameter from query", Property = nameof(GetAvailabilityCreatedEventsRequest.Site)});
            }

            if (!req.Query.TryGetValue("from", out var from))
            {
                errors.Add(new ErrorMessageResponseItem { Message = "Error parsing from parameter from query", Property = nameof(GetAvailabilityCreatedEventsRequest.From) });
            }

            var parsedRequest = new GetAvailabilityCreatedEventsRequest(site, from);

            return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, GetAvailabilityCreatedEventsRequest request)>((errors, parsedRequest));
        }
    }
