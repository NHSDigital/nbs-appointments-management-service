namespace Nhs.Appointments.Core;

public class AccessibilityDefinitionsService(IAccessibilityDefinitionsStore AccessibilityDefinitionsStore) : IAccessibilityDefinitionsService
{
    public Task<IEnumerable<AccessibilityDefinition>> GetAccessibilityDefinitions()
    {
        return AccessibilityDefinitionsStore.GetAccessibilityDefinitionsDocument();
    }
}
