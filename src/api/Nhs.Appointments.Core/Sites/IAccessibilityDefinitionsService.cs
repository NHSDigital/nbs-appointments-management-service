namespace Nhs.Appointments.Core.Sites;

public interface IAccessibilityDefinitionsService
{
    Task<IEnumerable<AccessibilityDefinition>> GetAccessibilityDefinitions();
}
