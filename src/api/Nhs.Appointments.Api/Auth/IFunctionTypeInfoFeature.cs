using System.Reflection;

namespace Nhs.Appointments.Api.Auth;

public interface IFunctionTypeInfoFeature
{
    MethodInfo EntryPointInfo { get; }

    public bool RequiresAuthentication { get; }
    public string RequiredPermission { get; }

    public System.Type RequestInspector { get; }
}

