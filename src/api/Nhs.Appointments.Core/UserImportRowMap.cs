using CsvHelper.Configuration;
using static Nhs.Appointments.Core.UserDataImportHandler;

namespace Nhs.Appointments.Core;

public class UserImportRowMap : ClassMap<UserImportRow>
{
    public UserImportRowMap()
    {
        var userRoleKeys = new[]
        {
            "appointments-manager",
            "availability-manager",
            "site-details-manager",
            "user-manager"
        };

        Map(m => m.UserId)
            .Name("User")
            .Validate(f => !string.IsNullOrWhiteSpace(f.Field));
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
                    roleAssignemnts.Add(new RoleAssignment { Role = $"cannjed:{role}", Scope = $"site:{x.Row.GetField("Site")}" });
                }
            }

            return roleAssignemnts;
        });
    }
}
