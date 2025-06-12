using System.Text.RegularExpressions;

namespace Nhs.Appointments.Core.Constants;
public partial class RegularExpressionConstants
{
    private const string GbCulture = "en-GB";

    private const string LandlineNumber =
         @"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$";
    private const string MobileNumber = @"^\s*07(?=\d{9,11}(\s*)$)[\d]+(?:\s[\d]+)*\s*$|^(?(?=(\s*(\+\(44\)|\(\+44\)|\+44)))\s*(\+\(44\)|\(\+44\)|\+44)\s?((07|7)(?=\d{9,11}(\s*)$)[\d]+(?:\s[\d]+)*\s*)|(?(?=(\s*(\+\(\d{2}\)|\(\+\d{2}\)|\+\d{2})))(\s*(\+\(\d{2}\)|\(\+\d{2}\)|\+\d{2}))\s?(?=.{8,19}(\s*)$)[\d]+(?:\s[\d]+)*\s*))$";
    private const string OdsCode = "^[A-Z0-9]{1,10}$";
    private const string EmailAddress = "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$";

    [GeneratedRegex(LandlineNumber, RegexOptions.IgnoreCase, GbCulture)]
    public static partial Regex LandlineNumberRegex();

    [GeneratedRegex(MobileNumber, RegexOptions.IgnoreCase, GbCulture)]
    public static partial Regex MobileNumberRegex();

    [GeneratedRegex(OdsCode, RegexOptions.None, GbCulture)]
    public static partial Regex OdsCodeRegex();

    [GeneratedRegex(EmailAddress, RegexOptions.IgnoreCase, GbCulture)]
    public static partial Regex EmailAddressRegex();
}
