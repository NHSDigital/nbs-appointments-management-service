using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool;

public class SiteMap : ClassMap<SiteDocument>
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
        Map(m => m.Id).TypeConverter<GuidStringTypeConverter>();
        Map(m => m.OdsCode).Name("OdsCode");
        Map(m => m.Name).Name("Name");
        Map(m => m.Address).Name("Address");
        Map(m => m.PhoneNumber).Name("PhoneNumber");
        Map(m => m.Location).Convert(x =>
        {
            var canParseLong = double.TryParse(x.Row.GetField("Longitude"), out var longitude);
            var canParseLat = double.TryParse(x.Row.GetField("Latitude"), out var latitude);

            return canParseLong && canParseLat
                ? new Location(
                "Point",
                [longitude, latitude]
                )
                : null;
        });
        Map(m => m.IntegratedCareBoard).Name("ICB");
        Map(m => m.Region).Name("Region");
        Map(m => m.Type).Name("Site type");
        Map(m => m.DocumentType).Constant("site");
        Map(m => m.Accessibilities).Convert(x =>
        {
            return accessibilityKeys
                .Select(key => new Accessibility($"accessibility/{key}",
                    ParseUserEnteredBoolean(x.Row[key]).ToString()))
                .ToArray();
        });
    }

    private static bool ParseUserEnteredBoolean(string possibleBool)
    {
        possibleBool = possibleBool?.ToLower();
        return possibleBool == "true" || possibleBool == "yes";
    }

    /// <summary>
    /// Custom TypeConverter to validate a string GUID
    /// </summary>
    private class GuidStringTypeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string guidString, IReaderRow row, MemberMapData memberMapData)
        {
            if (Guid.TryParse(guidString, out var guid))
            {
                return guid.ToString();
            }

            throw new TypeConverterException(this, memberMapData, guidString, row.Context,
                $"Invalid GUID string format: {guidString}");
        }
    }
}
