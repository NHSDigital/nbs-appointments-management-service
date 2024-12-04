namespace Nhs.Appointments.Core;

public class Scope
{
    public static string GetValue(string scopeType, string scope)
    {
        if(!scope.StartsWith(scopeType + ":"))
        {
            return string.Empty;
        }

        return scope.Split(':')[1];
    }
}
