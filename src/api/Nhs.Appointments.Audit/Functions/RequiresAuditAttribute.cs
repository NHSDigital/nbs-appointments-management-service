namespace Nhs.Appointments.Audit.Functions;

[AttributeUsage(AttributeTargets.Method)]
public class RequiresAuditAttribute(Type requestSiteInspector) : Attribute
{
    public Type RequestSiteInspector { get; } = requestSiteInspector;
}
