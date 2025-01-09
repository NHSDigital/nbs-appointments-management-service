namespace Nhs.Appointments.Audit;

[AttributeUsage(AttributeTargets.Method)]
public class RequiresAuditAttribute(Type? requestSiteInspector) : Attribute
{
    public Type? RequestSiteInspector { get; } = requestSiteInspector;
}
