using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Net;
using System.Threading.Tasks;
using Nhs.Appointments.Core.Inspectors;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using System;
using Nhs.Appointments.Api.Constants;

namespace Nhs.Appointments.Api.Functions;

public class BulkSiteImportFunction(IUserDataImportHandler userDataImportHandler, ISiteDataImportHandler siteDataImportHandler, IApiUserDataImportHandler apiUserDataImportHandler,
    IValidator<BulkImportRequest> validator, IUserContextProvider userContextProvider, ILogger<SetAvailabilityFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<BulkImportRequest, IEnumerable<ReportItem>>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "Bulk User Import", tags: ["User"], Summary = "Bulk import users")]
    [OpenApiRequestBody("application/json", typeof(SetAvailabilityRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Users successfully imported")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    // TODO: Add new permission - data importer?
    //[RequiresPermission(Permissions.SetupAvailability, typeof(NoSiteRequestInspector))]
    [Function("BulkUserImportFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{type}/import")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, BulkImportRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var files = req.Form.Files;
        var errors = new List<ErrorMessageResponseItem>();

        if (files.Count is 0)
        {
            errors.Add(new ErrorMessageResponseItem { Message = "No file uploaded in the request form." });
        }

        if (files.Count > 1)
        {
            errors.Add(new ErrorMessageResponseItem { Message = "Too many files uploaded. Only upload one file at a time." });
        }

        var type = req.HttpContext.GetRouteValue("type")?.ToString();
        var parsedRequest = new BulkImportRequest(files[0], type);

        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, BulkImportRequest request)>((errors, parsedRequest));
    }

    protected override async Task<ApiResult<IEnumerable<ReportItem>>> HandleRequest(BulkImportRequest request, ILogger logger)
    {
        var result = request.Type switch
        {
            BulkImportType.User => await userDataImportHandler.ProcessFile(request.File),
            BulkImportType.Site => await siteDataImportHandler.ProcessFile(request.File),
            BulkImportType.ApiUser => await apiUserDataImportHandler.ProcessFile(request.File),
            _ => throw new NotSupportedException($"Type: {request.Type} not supported."),
        };

        // TODO: Conversations around return object to determine what exactly we return here
        return ApiResult<IEnumerable<ReportItem>>.Success(result);
    }
}

