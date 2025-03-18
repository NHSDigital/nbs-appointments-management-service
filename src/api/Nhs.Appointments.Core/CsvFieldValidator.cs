using Nhs.Appointments.Core.Constants;

namespace Nhs.Appointments.Core;

public static class CsvFieldValidator
{
    public static bool StringHasValue(string value) => !string.IsNullOrWhiteSpace(value);

    public static bool ParseUserEnteredBoolean(string possibleBool)
    {
        return !bool.TryParse(possibleBool, out var result)
            ? throw new FormatException($"Invalid bool string format: {possibleBool}")
            : result;
    }

    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        return !string.IsNullOrWhiteSpace(phoneNumber)
            && (RegularExpressionConstants.LandlineNumberRegex().IsMatch(phoneNumber)
            || RegularExpressionConstants.MobileNumberRegex().IsMatch(phoneNumber));
    }
}
