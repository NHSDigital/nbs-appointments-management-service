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
using Nhs.Appointments.Api.Factories;

namespace Nhs.Appointments.Api.Functions;

public class BulkImportFunction(IDataImportHandlerFactory dataImportHandlerFactory, IValidator<BulkImportRequest> validator, IUserContextProvider userContextProvider, ILogger<BulkImportFunction> logger, IMetricsRecorder metricsRecorder)
    : BaseApiFunction<BulkImportRequest, IEnumerable<ReportItem>>(validator, userContextProvider, logger, metricsRecorder)
{
    [OpenApiOperation(operationId: "Bulk Import", tags: ["BulkImport"], Summary = "Bulk import of users and sites")]
    [OpenApiRequestBody("application/json", typeof(SetAvailabilityRequest), Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Data successfully imported")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", typeof(ErrorMessageResponseItem), Description = "The body of the request is invalid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, "application/json", typeof(ErrorMessageResponseItem), Description = "Unauthorized request to a protected API")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, "application/json", typeof(ErrorMessageResponseItem), Description = "Request failed due to insufficient permissions")]
    //[RequiresPermission(Permissions.SystemDataImporter, typeof(NoSiteRequestInspector))]
    [Function("BulkImportFunction")]
    public override Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{type}/import")] HttpRequest req)
    {
        return base.RunAsync(req);
    }

    protected override Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, BulkImportRequest request)> ReadRequestAsync(HttpRequest req)
    {
        var files = req.Form?.Files;
        var errors = new List<ErrorMessageResponseItem>();

        if (files is null || files?.Count is 0)
        {
            errors.Add(new ErrorMessageResponseItem { Message = "No file uploaded in the request form." });
            return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, BulkImportRequest request)>((errors, null));
        }

        if (files.Count > 1)
        {
            errors.Add(new ErrorMessageResponseItem { Message = "Too many files uploaded. Only upload one file at a time." });
            return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, BulkImportRequest request)>((errors, null));
        }

        var type = req.HttpContext.GetRouteValue("type")?.ToString();
        var parsedRequest = new BulkImportRequest(files[0], type);

        return Task.FromResult<(IReadOnlyCollection<ErrorMessageResponseItem> errors, BulkImportRequest request)>((errors, parsedRequest));
    }

    protected override async Task<ApiResult<IEnumerable<ReportItem>>> HandleRequest(BulkImportRequest request, ILogger logger)
    {
        var handler = dataImportHandlerFactory.CreateDataImportHandler(request.Type);
        var result = await handler.ProcessFile(request.File);

        // TODO: Conversations around return object to determine what exactly we return here
        return ApiResult<IEnumerable<ReportItem>>.Success(result);
    }
}

