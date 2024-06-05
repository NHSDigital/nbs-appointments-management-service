using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Nhs.Appointments.Api.Auth;

public class FunctionTypeInfoFeature(MethodInfo methodInfo) : IFunctionTypeInfoFeature
{
    private readonly MethodInfo _methodInfo = methodInfo;

    public MethodInfo EntryPointInfo => _methodInfo;

    public bool RequiresAuthentication => _methodInfo.GetCustomAttribute<AllowAnonymousAttribute>() == null;
    public bool RequiresPermission => _methodInfo.GetCustomAttribute<RequiresPermissionAttribute>() != null;
}

