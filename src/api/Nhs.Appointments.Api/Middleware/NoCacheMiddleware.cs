using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Middleware;
public class NoCacheMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        await next(context);

        var httpContext = context.GetHttpContext();
        if (httpContext is null)
        {
            return;
        }

        var response = httpContext.Response;
        response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        response.Headers["Pragma"] = "no-cache";
    }
}
