using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Nhs.Appointments.Api.Auth;

public class SiteInspectorMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();
        
        if (request.Query.AllKeys.Contains("site"))
        {
            var siteId = request.Query.Get("site");
            context.Items.Add("siteId", siteId);
        }
        else
        {
            var body = await request.ReadBodyAsStringAndLeaveIntactAsync();
            
            if (string.IsNullOrEmpty(body) == false)
            {
                try
                {
                    using var jsonDocument = JsonDocument.Parse(body);
                    var siteId = jsonDocument.RootElement.GetProperty("site").ToString();
                    context.Items.Add("siteId", siteId);
                }
                catch (JsonException)
                {
                    
                }
            }
        }
        
        await next(context);
    }
}
