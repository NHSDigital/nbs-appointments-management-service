using System.Reflection;

namespace Nhs.Appointments.Audit;

public class AuditFunctionTypeInfoFeature(MethodInfo methodInfo) : IAuditFunctionTypeInfoFeature
{
    private readonly Lazy<RequiresAuditAttribute> _requiresAuditAttribute =
        new(methodInfo.GetCustomAttribute<RequiresAuditAttribute>);

    public bool RequiresAudit => _requiresAuditAttribute?.Value != null;

    public Type? RequestSiteInspector => _requiresAuditAttribute.Value?.RequestSiteInspector;
}

public class SkipAuditFunctionTypeInfoFeature : IAuditFunctionTypeInfoFeature
{
    public bool RequiresAudit => false;

    public Type? RequestSiteInspector => null;
}
