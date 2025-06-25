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
        Map(m => m.Id).Convert(x =>
            {
                var site = x.Row.GetField<string>("Id");

                if (!CsvFieldValidator.StringHasValue(site))
                    throw new ArgumentException("Site ID must have a value.");

                return !Guid.TryParse(site, out _)
                        ? throw new ArgumentException($"Invalid GUID string format for Site field: '{site}'")
                        : site;
            });
        Map(m => m.OdsCode).Convert(x =>
        {
            var odsCode = x.Row.GetField<string>("OdsCode");

            if (!CsvFieldValidator.StringHasValue(odsCode))
                throw new ArgumentException("OdsCode must have a value.");

            if (!RegularExpressionConstants.OdsCodeRegex().IsMatch(odsCode))
                throw new ArgumentException($"OdsCode: '{odsCode}' is invalid. OdsCode's must be a maximum of 10 characters long and only contain numbers and capital letters.");

            return odsCode;
        });
        Map(m => m.Name).Convert(x =>
        {
            var name = x.Row.GetField<string>("Name");

            if (!CsvFieldValidator.StringHasValue(name))
                throw new ArgumentException("Site name must have a value.");

            return name;
        });
        Map(m => m.Address).Convert(x =>
        {
            var address = x.Row.GetField<string>("Address");

            if (!CsvFieldValidator.StringHasValue(address))
                throw new ArgumentException("Site address must have a value.");

            return address;
        });
        Map(m => m.PhoneNumber).Convert(x =>
        {
            var phoneNumber = x.Row.GetField<string>("PhoneNumber");

            if (!CsvFieldValidator.StringHasValue(phoneNumber))
                throw new ArgumentException("Phone number must have a value.");

            if (!CsvFieldValidator.IsValidPhoneNumber(phoneNumber))
                throw new ArgumentException($"Phone number must be a valid phone number or 'N'. Current phone number: '{phoneNumber}'");

            return phoneNumber;
        });
        Map(m => m.Location).Convert(x =>
        {
            var longitude = x.Row.GetField<double>("Longitude");
            var latitude = x.Row.GetField<double>("Latitude");

            if (longitude is > MaxLongitude or < MinLongitude)
                throw new ArgumentOutOfRangeException($"Longitude: '{longitude}' is not a valid UK longitude.");

            if (latitude is > MaxLatitude or < MinLatitude)
                throw new ArgumentOutOfRangeException($"Latitude: '{latitude}' is not a valid UK latitude.");

            return new Location("Point", [longitude, latitude]);
        });
        Map(m => m.ICB).Convert(x =>
        {
            var icb = x.Row.GetField<string>("ICB");

            if (!CsvFieldValidator.StringHasValue(icb))
                throw new ArgumentException("ICB must have a value.");

            return icb;
        });
        Map(m => m.Region).Convert(x =>
        {
            var region = x.Row.GetField<string>("Region");

            if (!CsvFieldValidator.StringHasValue(region))
                throw new ArgumentException("Region must have a value.");

            return region;
        });
        Map(m => m.Accessibilities).Convert(x =>
        {
            return accessibilityKeys
                .Select(key => new Accessibility($"accessibility/{key}",
                    CsvFieldValidator.ParseUserEnteredBoolean(x.Row[key]).ToString()))
                .ToArray();
        });
        Map(m => m.Type).Convert(x => x.Row.GetField<string>("Site type"));
    }
}
