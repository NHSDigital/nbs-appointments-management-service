using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using System.Text.RegularExpressions;

namespace CsvDataTool;

public class SiteMap : ClassMap<SiteDocument>
{
    public SiteMap()
    {
        //validate ID provided is a GUID
        Map(m => m.Id)
            .TypeConverter<GuidStringTypeConverter>();
        Map(m => m.OdsCode)
            .Name("OdsCode")
            .Validate(f => StringHasValue(f.Field));
        Map(m => m.Name)
            .Name("Name")
            .Validate(f => StringHasValue(f.Field));
        Map(m => m.Address)
            .Name("Address")
            .Validate(f => StringHasValue(f.Field));
        Map(m => m.PhoneNumber)
            .Name("PhoneNumber")
            .Validate(f => IsValidPhoneNumber(f.Field));
        Map(m => m.Location).Convert(x =>
            new Location(
                "Point",
                [x.Row.GetField<double>("Longitude"), x.Row.GetField<double>("Latitude")]
            ));
        Map(m => m.IntegratedCareBoard)
            .Name("ICB")
            .Validate(f => StringHasValue(f.Field));
        Map(m => m.Region)
            .Name("Region")
            .Validate(f => StringHasValue(f.Field));
        Map(m => m.DocumentType).Constant("site");
        Map(m => m.AttributeValues).Convert(x =>
        {
            AttributeValue[] result =
            [
                new AttributeValue("accessibility/accessible_toilet",
                    ParseUserEnteredBoolean(x.Row["accessible_toilet"] ).ToString()),
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
        return !bool.TryParse(possibleBool, out var result)
            ? throw new Exception($"Invalid bool string format: {possibleBool}") // TODO: Throw a more accurate exception
            : result;
    }

    private static bool StringHasValue(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        return !string.IsNullOrWhiteSpace(phoneNumber) && RegularExpressionConstants.LandlineNumberRegex().IsMatch(phoneNumber);
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

public partial class RegularExpressionConstants
{
    // TODO: Will sites always have a landline number or could they use a mobile number?
    private const string LandlineNumber =
         @"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$";

    [GeneratedRegex(LandlineNumber, RegexOptions.IgnoreCase, "en-GB")]
    public static partial Regex LandlineNumberRegex();
}
