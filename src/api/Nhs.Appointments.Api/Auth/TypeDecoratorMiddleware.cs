using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Json;

namespace Nhs.Appointments.Api.Auth;

public class TypeDecoratorMiddleware : IFunctionsWorkerMiddleware
{
    public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {        
        var typeName = String.Join(".", context.FunctionDefinition.EntryPoint.Split('.')[..^1]);
        var methodName = context.FunctionDefinition.EntryPoint.Split('.')[^1];
        var type = Type.GetType(typeName);
        var methodInfo = type.GetMethod(methodName);
        context.Features.Set<IFunctionTypeInfoFeature>(new FunctionTypeInfoFeature(methodInfo));
        return next(context);
    }
}

public class AuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var functionTypeInfoFeature = context.Features.Get<IFunctionTypeInfoFeature>();
        if(functionTypeInfoFeature.RequiresPermission == false)
        {
            // var requiredPermission = functionTypeInfoFeature.RequiredPermission;
            // var isAuthorized = _permissionChecker.HasPermission(userId, requiredPermission);
            // if (isAuthorized)
            // {
                await next(context);
                return;
        //     }
        }
        HandleUnauthorizedAccess(context);
    }
    
    protected virtual void HandleUnauthorizedAccess(FunctionContext context)
    {
        var resultBinding = context.GetOutputBindings<IActionResult>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name == "$return");
        resultBinding.Value = JsonResponseWriter.WriteResult(null, HttpStatusCode.Forbidden);
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class RequiresPermissionAttribute(string permission) : Attribute
{
    public string Permission { get; } = permission;
}

