namespace Nhs.Appointments.Core;

public interface IAccessibilityDefinitionsStore
{
    Task<IEnumerable<AccessibilityDefinition>> GetAccessibilityDefinitionsDocument();
}
