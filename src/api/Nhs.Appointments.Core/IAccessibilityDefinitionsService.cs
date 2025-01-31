namespace Nhs.Appointments.Core;

public interface IAccessibilityDefinitionsService
{
    Task<IEnumerable<AccessibilityDefinition>> GetAccessibilityDefinitions();
}
