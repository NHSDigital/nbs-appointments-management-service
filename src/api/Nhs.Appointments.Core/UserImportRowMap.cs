using CsvHelper;
using CsvHelper.Configuration;
using static Nhs.Appointments.Core.UserDataImportHandler;

namespace Nhs.Appointments.Core;

public class UserImportRowMap : ClassMap<UserImportRow>
{
    private readonly bool _oktaEnabled;
    public UserImportRowMap(bool oktaEnabled)
    {
        _oktaEnabled = oktaEnabled;

        var userRoleKeys = new[]
        {
            "appointment-manager",
            "availability-manager",
            "site-details-manager",
            "user-manager"
        };

        Map(m => m.UserId)
            .Name("User")
            .Validate(f => ValidateUser(f.Row));
        Map(m => m.FirstName)
            .Name("FirstName")
            .Validate(f => ValidateName(f.Row, "FirstName"));
        Map(m => m.LastName)
            .Name("LastName")
            .Validate(f => ValidateName(f.Row, "LastName"));
        Map(m => m.SiteId)
            .Name("Site")
            .TypeConverter<GuidStringTypeConverter>();
        Map(m => m.RoleAssignments).Convert(x =>
        {
            var roleAssignemnts = new List<RoleAssignment>();
            foreach (var role in userRoleKeys)
            {
                if (CsvFieldValidator.ParseUserEnteredBoolean(x.Row.GetField(role)))
                {
                    roleAssignemnts.Add(new RoleAssignment { Role = $"canned:{role}", Scope = $"site:{x.Row.GetField("Site")}" });
                }
            }

            return roleAssignemnts;
        });
    }

    private static bool IsOktaUser(IReaderRow row)
    {
        var userId = row.GetField("User");
        return !userId.EndsWith("@nhs.net", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ValidateName(IReaderRow row, string fieldName)
    {
        if (IsOktaUser(row))
        {
            var name = row.GetField(fieldName);
            return CsvFieldValidator.StringHasValue(name)
                ? true
                : throw new ArgumentNullException($"OKTA user must have {fieldName} set.");
        }
        return true; // No validation required for non-okta users
    }

    private bool ValidateUser(IReaderRow row)
    {
        var user = row.GetField("User");

        if (!CsvFieldValidator.StringHasValue(user))
            throw new ArgumentNullException("User must have a value.");

        if (!_oktaEnabled && IsOktaUser(row))
            throw new ArgumentException($"User: {user} is an OKTA user and OKTA is not enabled.");
        
        return true;
    }
}
