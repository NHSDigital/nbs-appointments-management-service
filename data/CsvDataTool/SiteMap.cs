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
        //validate ID provided is a GUID
        Map(m => m.Id).TypeConverter<GuidStringTypeConverter>();
        Map(m => m.OdsCode).Name("OdsCode");
        Map(m => m.Name).Name("Name");
        Map(m => m.Address).Name("Address");
        Map(m => m.PhoneNumber).Name("PhoneNumber");
        Map(m => m.Location).Convert(x =>
            new Location(
                "Point",
                [x.Row.GetField<double>("Longitude"), x.Row.GetField<double>("Latitude")]
            ));
        Map(m => m.IntegratedCareBoard).Name("ICB");
        Map(m => m.Region).Name("Region");
        Map(m => m.DocumentType).Constant("site");
        Map(m => m.AttributeValues).Convert(x =>
        {
            AttributeValue[] result =
            [
                new AttributeValue("accessibility/accessible_toilet",
                    ParseUserEnteredBoolean(x.Row["accessible_toilet"]).ToString()),
                new AttributeValue("accessibility/braille_translation_service",
                    ParseUserEnteredBoolean(x.Row["braille_translation_service"]).ToString()),
                new AttributeValue("accessibility/disabled_car_parking",
                    ParseUserEnteredBoolean(x.Row["disabled_car_parking"]).ToString()),
                new AttributeValue("accessibility/car_parking",
                    ParseUserEnteredBoolean(x.Row["car_parking"]).ToString()),
                new AttributeValue("accessibility/induction_loop",
                    ParseUserEnteredBoolean(x.Row["induction_loop"]).ToString()),
                new AttributeValue("accessibility/sign_language_service",
                    ParseUserEnteredBoolean(x.Row["sign_language_service"]).ToString()),
                new AttributeValue("accessibility/step_free_access",
                    ParseUserEnteredBoolean(x.Row["step_free_access"]).ToString()),
                new AttributeValue("accessibility/text_relay", ParseUserEnteredBoolean(x.Row["text_relay"]).ToString()),
                new AttributeValue("accessibility/wheelchair_access",
                    ParseUserEnteredBoolean(x.Row["wheelchair_access"]).ToString()),
            ];

            return result;
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
