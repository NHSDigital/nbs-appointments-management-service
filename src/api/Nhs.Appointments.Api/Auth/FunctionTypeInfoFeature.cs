using Microsoft.AspNetCore.Authorization;
using System;
using System.Reflection;

namespace Nhs.Appointments.Api.Auth;

public class FunctionTypeInfoFeature(MethodInfo methodInfo) : IFunctionTypeInfoFeature
{    
    private readonly Lazy<AllowAnonymousAttribute> _allowAnonymousAttribute = new Lazy<AllowAnonymousAttribute>(methodInfo.GetCustomAttribute<AllowAnonymousAttribute>);
    private readonly Lazy<RequiresPermissionAttribute> _requiresPermissionAttribute = new Lazy<RequiresPermissionAttribute>(methodInfo.GetCustomAttribute<RequiresPermissionAttribute>);

    public MethodInfo EntryPointInfo => methodInfo;

    public bool RequiresAuthentication => _allowAnonymousAttribute.Value == null;
    public string RequiredPermission => _requiresPermissionAttribute.Value?.Permission;
    public Type RequestInspector => _requiresPermissionAttribute.Value?.RequestInspector;
}

public class SkipInfoFeature : IFunctionTypeInfoFeature
{

    public MethodInfo EntryPointInfo => null;
    public bool RequiresAuthentication => false;
    public string RequiredPermission => String.Empty;
    public Type RequestInspector => null;
}

