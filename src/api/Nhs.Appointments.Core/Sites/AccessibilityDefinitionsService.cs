namespace Nhs.Appointments.Core.Sites;

public class AccessibilityDefinitionsService(IAccessibilityDefinitionsStore AccessibilityDefinitionsStore) : IAccessibilityDefinitionsService
{
    public async Task<IEnumerable<AccessibilityDefinition>> GetAccessibilityDefinitions()
    {
        return await AccessibilityDefinitionsStore.GetAccessibilityDefinitionsDocument();
    }
}
