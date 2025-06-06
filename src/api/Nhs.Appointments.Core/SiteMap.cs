using CsvHelper.Configuration;
using Nhs.Appointments.Core.Constants;
using static Nhs.Appointments.Core.SiteDataImporterHandler;

namespace Nhs.Appointments.Core;

public class SiteMap : ClassMap<SiteImportRow>
{
    // Upper and lower boundaries for longitude & latitudes for the UK
    private const double MaxLatitude = 60.9;
    private const double MinLatitude = 49.8;
    private const double MaxLongitude = 1.8;
    private const double MinLongitude = -8.1;

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
        Map(m => m.OdsCode).Convert(x =>
        {
            var odsCode = x.Row.GetField<string>("OdsCode");

            if (!CsvFieldValidator.StringHasValue(odsCode))
                throw new ArgumentException("OdsCode must have a value.");

            if (!RegularExpressionConstants.OdsCodeRegex().IsMatch(odsCode))
                throw new ArgumentException($"OdsCode: {odsCode} is invalid. OdsCode's must be a maximum of 10 characters long and only contain numbers and capital letters.");

            return odsCode;
        });
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
        {
            var longitude = x.Row.GetField<double>("Longitude");
            var latitude = x.Row.GetField<double>("Latitude");

            if (longitude is > MaxLongitude or < MinLongitude)
                throw new ArgumentOutOfRangeException($"Longitude: {longitude} is not a valid UK longitude.");

            if (latitude is > MaxLatitude or < MinLatitude)
                throw new ArgumentOutOfRangeException($"Latitude: {latitude} is not a valid UK latitude.");

            return new Location("Point", [longitude, latitude]);
        });
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
