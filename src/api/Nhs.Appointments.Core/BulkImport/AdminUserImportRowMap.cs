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

            if (!CsvFieldValidator.StringHasValue(email))
            {
                throw new ArgumentNullException("Email field must have a value.");
            }

            if (!RegularExpressionConstants.EmailAddressRegex().IsMatch(email))
            {
                throw new ArgumentException($"Admin user email: '{email}' is not a valid email address.");
            }

            if (!email.ToLower().EndsWith("nhs.net"))
            {
                throw new ArgumentException($"Email must be an nhs.net email domain. Current email: '{email}'");
            }

            return email;
        });
    }
}
