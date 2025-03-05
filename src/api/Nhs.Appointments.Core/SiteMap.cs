using CsvHelper.Configuration;
using static Nhs.Appointments.Core.SiteDataImporterHandler;

namespace Nhs.Appointments.Core;

public class SiteMap : ClassMap<SiteImportRow>
{
    public SiteMap()
    {
        var accessibilityKeys = new[]
        {
            "accessible_toilet",
            "braille_translation_service",
            "disabled_car_parking",
            "car_parking",
            "induction_loop",
            "sign_language_service",
            "step_free_access",
            "text_relay",
            "wheelchair_access"
        };

        //validate ID provided is a GUID
        Map(m => m.Id)
            .TypeConverter<GuidStringTypeConverter>();
        Map(m => m.OdsCode)
            .Name("OdsCode")
            .Validate(f => CsvFieldValidator.StringHasValue(f.Field));
        Map(m => m.Name)
            .Name("Name")
            .Validate(f => CsvFieldValidator.StringHasValue(f.Field));
        Map(m => m.Address)
            .Name("Address")
            .Validate(f => CsvFieldValidator.StringHasValue(f.Field));
        Map(m => m.PhoneNumber)
            .Name("PhoneNumber")
            .Validate(f => CsvFieldValidator.StringHasValue(f.Field) && CsvFieldValidator.IsValidPhoneNumber(f.Field));
        Map(m => m.Location).Convert(x =>
            new Location(
                "Point",
                [x.Row.GetField<double>("Longitude"), x.Row.GetField<double>("Latitude")]
            ));
        Map(m => m.ICB)
            .Name("ICB")
            .Validate(f => CsvFieldValidator.StringHasValue(f.Field));
        Map(m => m.Region).Name("Region")
            .Validate(f => CsvFieldValidator.StringHasValue(f.Field));
        Map(m => m.Accessibilities).Convert(x =>
        {
            return accessibilityKeys
                .Select(key => new Accessibility($"accessibility/{key}",
                    CsvFieldValidator.ParseUserEnteredBoolean(x.Row[key]).ToString()))
                .ToArray();
        });
    }
}
