using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Nhs.Appointments.Api.Functions;

public abstract class BaseApiFunction<TRequest, TResponse>
{
    private readonly IRequestAuthenticatorFactory _authenticatorFactory;
    private readonly IValidator<TRequest> _validator;
    private ClaimsPrincipal _principal;
    private readonly ILogger _logger;

    protected BaseApiFunction(IValidator<TRequest> validator, IRequestAuthenticatorFactory authenticatorFactory, ILogger logger)
    {
        _validator = validator;
        _authenticatorFactory = authenticatorFactory;
        _logger = logger;
    }

    public virtual async Task<IActionResult> RunAsync(HttpRequest req)
    {
        try
        {
            var authenticated = await AuthenticateRequest(req);
            if(authenticated == false)
                return ProblemResponse(HttpStatusCode.Unauthorized, string.Empty);

            (bool requestRead, TRequest request) = await ReadRequestAsync(req);
            if (requestRead == false)
                return ProblemResponse(HttpStatusCode.BadRequest, "The request was invalid");

            var validationErrors = await ValidateRequest(request);
            if (validationErrors.Any())
                return ProblemResponse(HttpStatusCode.BadRequest, validationErrors);

            var response = await HandleRequest(request, _logger);
            if (response.IsSuccess)
            {
                if (response.ResponseObject is EmptyResponse)
                {
                    return new OkResult();
                }
                return JsonResponseWriter.WriteResult(response.ResponseObject);
            }
            else
                return ProblemResponse(response.StatusCode, response.Reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new InternalServerErrorResult();
        }
    }    

    private async Task<bool> AuthenticateRequest(HttpRequest req)
    {
        var authHeaderValue = req.Headers["Authorization"].FirstOrDefault();
        if (authHeaderValue == null)
            return false;

        var parts = authHeaderValue.Split(' ');
        var scheme = parts[0];
        var value = parts[1];
        
        try
        {            
            var authenticator = _authenticatorFactory.CreateAuthenticator(scheme);
            _principal = await authenticator.AuthenticateRequest(value);
            return _principal.Identity.IsAuthenticated;
        }
        catch(NotSupportedException)
        {
            return false;
        }        
    }

    protected virtual Task<(bool requestRead, TRequest request)> ReadRequestAsync(HttpRequest req) => JsonRequestReader.TryReadRequestAsync<TRequest>(req.Body);

    protected virtual async Task<IEnumerable<ErrorMessageResponseItem>> ValidateRequest(TRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return
                validationResult.Errors
                    .Select(x => new ErrorMessageResponseItem() { Message = x.ErrorMessage, Property = x.PropertyName });                        
        }

        return Enumerable.Empty<ErrorMessageResponseItem>();
    }

    private IActionResult ProblemResponse(HttpStatusCode status, object errorDetails)
    {
        return JsonResponseWriter.WriteResult(errorDetails, status);
    }


    private IActionResult ProblemResponse(HttpStatusCode status, string message)
    {
        var error = new
        {
            message
        };
        return ProblemResponse(status, error);
    }

    protected ApiResult<TResponse> Success(TResponse response) => ApiResult<TResponse>.Success(response);

    protected ApiResult<TResponse> Failed(HttpStatusCode status, string message) => ApiResult<TResponse>.Failed(status, message);
    
    protected ApiResult<EmptyResponse> Success() => ApiResult<EmptyResponse>.Success(new EmptyResponse());

    protected abstract Task<ApiResult<TResponse>> HandleRequest(TRequest request, ILogger logger);

    protected ClaimsPrincipal Principal => _principal;
}
