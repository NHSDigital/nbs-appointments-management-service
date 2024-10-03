using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MassTransit;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Api.Errors;

public sealed class ConcurrencyErrorHandlingMiddleware (ILogger<ConcurrencyErrorHandlingMiddleware> logger) : IFunctionsWorkerMiddleware
{


    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ConcurrencyException error)
        {
            logger.LogError(error.InnerException, error.Message);
            var request = await context.GetHttpRequestDataAsync();
            var response = request?.CreateResponse(HttpStatusCode.Conflict);
            response?.WriteString($"{{ \"error\": \"{error.Message}\"}}");
            context.GetInvocationResult().Value = response;
        }
    }
}
