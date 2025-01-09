using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Audit;

namespace Nhs.Appointments.Api;

public class TypeDecoratorMiddleware : IFunctionsWorkerMiddleware
{
    public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var methodsToSkip = new[] { "RenderSwaggerUI", "RenderSwaggerDocument" };
        var typeName = string.Join(".", context.FunctionDefinition.EntryPoint.Split('.')[..^1]);
        var methodName = context.FunctionDefinition.EntryPoint.Split('.')[^1];

        if (methodsToSkip.Contains(methodName))
        {
            context.Features.Set<IFunctionTypeInfoFeature>(new SkipInfoFeature());
            context.Features.Set<IAuditFunctionTypeInfoFeature>(new SkipAuditFunctionTypeInfoFeature());
        }
        else
        {
            var type = Type.GetType(typeName);
            var methodInfo = type.GetMethod(methodName);
            context.Features.Set<IFunctionTypeInfoFeature>(new FunctionTypeInfoFeature(methodInfo));
            context.Features.Set<IAuditFunctionTypeInfoFeature>(new AuditFunctionTypeInfoFeature(methodInfo));
        }
        
        return next(context);
    }
}
