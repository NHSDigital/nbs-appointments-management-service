using CsvHelper.Configuration;
using Nhs.Appointments.Core.Constants;
using static Nhs.Appointments.Core.BulkImport.AdminUserDataImportHandler;

namespace Nhs.Appointments.Core.BulkImport;
public class AdminUserImportRowMap : ClassMap<AdminUserImportRow>
{
    public AdminUserImportRowMap()
    {
        Map(m => m.Email).Convert(x =>
        {
            var email = x.Row.GetField<string>("Email");

            if (CsvFieldValidator.StringHasValue(email))
            {
                return RegularExpressionConstants.EmailAddressRegex().IsMatch(email)
                    ? email
                    : throw new ArgumentException($"Admin user email: {email} is not a valid email address.");
            }

            throw new ArgumentNullException("Email field must have a value");
        });
    }
}
