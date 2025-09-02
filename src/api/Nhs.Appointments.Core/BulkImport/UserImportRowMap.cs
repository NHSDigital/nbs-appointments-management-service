using CsvHelper;
using CsvHelper.Configuration;
using Nhs.Appointments.Core.Constants;
using static Nhs.Appointments.Core.BulkImport.UserDataImportHandler;

namespace Nhs.Appointments.Core.BulkImport;

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
            .Convert(x =>
            {
                var siteValue = x.Row.GetField<string>("Site");
                var regionValue = x.Row.GetField<string>("Region");
                var icbValue = x.Row.GetField<string>("ICB");

                ValidatePermissionScope(siteValue, regionValue, icbValue);

                if (CsvFieldValidator.StringHasValue(siteValue))
                {
                    return !Guid.TryParse(siteValue, out _)
                        ? throw new ArgumentException($"Invalid Guid string format for Site field: '{siteValue}'")
                        : siteValue;
                }

                return string.Empty;
            });
        Map(m => m.Region)
            .Convert(x =>
            {
                var siteValue = x.Row.GetField<string>("Site");
                var regionValue = x.Row.GetField<string>("Region");
                var icbValue = x.Row.GetField<string>("ICB");

                ValidatePermissionScope(siteValue, regionValue, icbValue);

                return regionValue;
            });
        Map(m => m.RoleAssignments).Convert(x =>
        {
            var roleAssignemnts = new List<RoleAssignment>();

            var siteValue = x.Row.GetField<string>("Site");
            var regionValue = x.Row.GetField<string>("Region");
            var icbValue = x.Row.GetField<string>("ICB");

            // No need to check again whether both or neither fields are populated here as that happens in the site / region mapping
            if (CsvFieldValidator.StringHasValue(siteValue))
            {
                foreach (var role in userRoleKeys)
                {
                    if (CsvFieldValidator.ParseUserEnteredBoolean(x.Row.GetField(role)))
                    {
                        roleAssignemnts.Add(new RoleAssignment { Role = $"canned:{role}", Scope = $"site:{siteValue}" });
                    }
                }

                return roleAssignemnts;
            }

            if (CsvFieldValidator.StringHasValue(regionValue))
            {
                roleAssignemnts.Add(new RoleAssignment { Role = "system:regional-user", Scope = $"region:{regionValue}" });
                return roleAssignemnts;
            }

            roleAssignemnts.Add(new RoleAssignment { Role = "system:icb-user", Scope = $"icb:{icbValue}" });
            return roleAssignemnts;
        });
        Map(m => m.Icb).Convert(x =>
        {
            var siteValue = x.Row.GetField<string>("Site");
            var regionValue = x.Row.GetField<string>("Region");
            var icbValue = x.Row.GetField<string>("ICB");

            ValidatePermissionScope(siteValue, regionValue, icbValue);

            return icbValue;
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
            throw new ArgumentException($"User: '{user}' is an OKTA user and OKTA is not enabled.");

        if (!RegularExpressionConstants.EmailAddressRegex().IsMatch(user))
            throw new ArgumentException($"User: '{user}' is not a valid email address");

        return true;
    }

    // Only one of the Site, Region or ICB fields can be populated
    private static void ValidatePermissionScope(string siteValue, string regionValue, string icbValue)
    {
        var fields = new Dictionary<string, bool>
        {
            { "Site", CsvFieldValidator.StringHasValue(siteValue) },
            { "Region", CsvFieldValidator.StringHasValue(regionValue) },
            { "ICB", CsvFieldValidator.StringHasValue(icbValue) }
        };

        var populatedFieldCount = fields.Count(f => f.Value);

        if (populatedFieldCount != 1)
        {
            var populatedFields = fields.Where(f => f.Value).Select(f => f.Key);
            var message = populatedFieldCount == 0
                ? "Exactly one of Site, Region or ICB must be populated, but none were provided."
                : $"Exactly one of Site, Region or ICB must be populated, but the populated fields were: {populatedFieldCount}: {string.Join(',', populatedFields)}";

            throw new ArgumentException(message);
        }
    }
}
