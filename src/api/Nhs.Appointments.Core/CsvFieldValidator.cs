using Nhs.Appointments.Core.Constants;

namespace Nhs.Appointments.Core;

public static class CsvFieldValidator
{
    public static bool StringHasValue(string value) => !string.IsNullOrWhiteSpace(value);

    public static bool ParseUserEnteredBoolean(string possibleBool)
    {
        return !bool.TryParse(possibleBool, out var result)
            ? throw new FormatException($"Invalid bool string format: '{possibleBool}'")
            : result;
    }

    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        // 'N' is valid for the bulk import in the scenario we don't have a number for that site
        return phoneNumber == "N" || (!string.IsNullOrWhiteSpace(phoneNumber)
            && (RegularExpressionConstants.LandlineNumberRegex().IsMatch(phoneNumber) || RegularExpressionConstants.MobileNumberRegex().IsMatch(phoneNumber)));
    }
}
