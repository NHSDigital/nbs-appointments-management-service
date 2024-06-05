using System.Collections.Generic;
using System.Reflection;

namespace Nhs.Appointments.Api.Auth;

public interface IFunctionTypeInfoFeature
{
    MethodInfo EntryPointInfo { get; }

    public bool RequiresAuthentication { get; }
    public bool RequiresPermission { get; }
}

