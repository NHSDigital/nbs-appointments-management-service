using System;

namespace Nhs.Appointments.Api.Auth;

[AttributeUsage(AttributeTargets.Method)]
public class RequiresPermissionAttribute(string permission) : Attribute
{
    public string Permission { get; } = permission;
}
