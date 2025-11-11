namespace Nhs.Appointments.Core.Sites;

public interface IAccessibilityDefinitionsStore
{
    Task<IEnumerable<AccessibilityDefinition>> GetAccessibilityDefinitionsDocument();
}
