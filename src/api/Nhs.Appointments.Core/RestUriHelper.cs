namespace Nhs.Appointments.Core;

public static class RestUriHelper
{
    public static string GetResourceIdFromPath(string path, string resource)
    {
        var pathSegments = path.Split("/").ToList();
        var index = pathSegments.IndexOf(resource);
        return pathSegments[index+1];
    }
}
