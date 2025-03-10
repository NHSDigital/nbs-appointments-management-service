using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Functions;

public abstract class BaseApiFunction<TRequest, TResponse>(
    IValidator<TRequest> validator,
    IUserContextProvider userContextProvider,
    ILogger logger,
    IMetricsRecorder metricsRecorder)
{
    protected ClaimsPrincipal Principal => userContextProvider.UserPrincipal;

    public virtual async Task<IActionResult> RunAsync(HttpRequest req)
    {
        try
        {
            var (errors, request) = await ReadRequestAsync(req);
            if (errors.Any())
            {
                return ProblemResponse(HttpStatusCode.BadRequest, errors);
            }

            var validationErrors = await ValidateRequest(request);
            if (validationErrors.Any())
            {
                return ProblemResponse(HttpStatusCode.BadRequest, validationErrors);
            }

            ApiResult<TResponse> response;
            using (metricsRecorder.BeginScope(GetType().Name))
            {
                response = await HandleRequest(request, logger);
            }

            WriteMetricsToConsole();

            if (response.IsSuccess)
            {
                if (response.ResponseObject is EmptyResponse)
                {
                    return new OkResult();
                }

                return JsonResponseWriter.WriteResult(response.ResponseObject);
            }

            return ProblemResponse(response.StatusCode, response.Reason);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return new InternalServerErrorResult();
        }
    }

    protected virtual Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, TRequest request)> ReadRequestAsync(
        HttpRequest req)
        => JsonRequestReader.ReadRequestAsync<TRequest>(req.Body);

    protected virtual async Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(TRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return
                validationResult.Errors
                    .Select(x => new ErrorMessageResponseItem { Message = x.ErrorMessage, Property = x.PropertyName });
        }

        return [];
    }

    private IActionResult ProblemResponse(HttpStatusCode status, object errorDetails)
    {
        return JsonResponseWriter.WriteResult(errorDetails, status);
    }

    private IActionResult ProblemResponse(HttpStatusCode status, string message)
    {
        var error = new { message };
        return ProblemResponse(status, error);
    }

    private void WriteMetricsToConsole()
    {
        if (metricsRecorder.Metrics != null)
        {
            foreach (var metric in metricsRecorder.Metrics)
            {
                Console.WriteLine(metric.Path + ": " + metric.Value);
            }
        }
    }

    protected ApiResult<TResponse> Success(TResponse response) => ApiResult<TResponse>.Success(response);

    protected ApiResult<TResponse> Failed(HttpStatusCode status, string message) =>
        ApiResult<TResponse>.Failed(status, message);

    protected ApiResult<EmptyResponse> Success() => ApiResult<EmptyResponse>.Success(new EmptyResponse());

    protected abstract Task<ApiResult<TResponse>> HandleRequest(TRequest request, ILogger logger);
}
