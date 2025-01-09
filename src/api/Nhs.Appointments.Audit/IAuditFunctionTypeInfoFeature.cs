namespace Nhs.Appointments.Audit;

public interface IAuditFunctionTypeInfoFeature
{
    public bool RequiresAudit { get; }

    public Type? RequestSiteInspector { get; }
}
