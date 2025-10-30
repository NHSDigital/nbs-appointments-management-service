using CsvHelper.Configuration;
using static Nhs.Appointments.Core.BulkImport.SiteStatusDataImportHandler;

namespace Nhs.Appointments.Core.BulkImport;
public class SiteStatusImportRowMap : ClassMap<SiteStatusImportRow>
{
    public SiteStatusImportRowMap()
    {
        Map(m => m.Id).Convert(x =>
        {
            var site = x.Row.GetField<string>("Id");

            if (!CsvFieldValidator.StringHasValue(site))
                throw new ArgumentException("Site ID must have a value.");

            return !Guid.TryParse(site, out _)
                    ? throw new ArgumentException($"Invalid GUID string format for Site field: '{site}'")
                    : site;
        });
        Map(m => m.Name).Convert(x =>
        {
            var name = x.Row.GetField<string>("Name");

            if (!CsvFieldValidator.StringHasValue(name))
                throw new ArgumentException("Site name must have a value.");

            return name;
        });
    }
}
