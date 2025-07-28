using System;

namespace Nhs.Appointments.Api.Auth;

[AttributeUsage(AttributeTargets.Method)]
public class RequiresPermissionAttribute(string permission, Type requestInspector) : Attribute
{
    public string Permission { get; } = permission;

    public Type RequestInspector { get; } = requestInspector;
}
