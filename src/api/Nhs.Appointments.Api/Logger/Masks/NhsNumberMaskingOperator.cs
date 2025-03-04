using Serilog.Enrichers.Sensitive;
using System.Text.RegularExpressions;

namespace Nhs.Appointments.Api.Logger.Masks;
/// <summary>
/// Serilog mask to prevent NHS Numbers (strings of exactly 10 digits) from being logged.
/// </summary>
/// <remarks>
/// This will match ALL 10 digit numbers. These can occur in GUIDs used by the RequestId and CorrelationId properties
/// attached to log messages.
/// </remarks>
public class NhsNumberMaskingOperator : RegexMaskingOperator
{
    /// <summary>
    /// Matches 10-digit numbers which are preceeded and followed by a non-digit character or line start/end.
    /// The "start" and "end" groups will contain the single character (if present) before and after the 10-digit string.
    /// </summary>
    private const string NhsNumberRegex = @"(?<start>^|[^0-9])(?<nhsnumber>[0-9]{10})(?<end>[^0-9]|$)";

    public NhsNumberMaskingOperator() : base(NhsNumberRegex)
    {
        // Nothing to do.
    }

    protected override string PreprocessMask(string mask, Match match)
    {
        var nhsNumberLast3Digits = match.Groups["nhsnumber"].Value[^3..];

        // Regex will match the characters immediately before and after the 10-digit string so
        // need to add them back in.
        return string.Concat(match.Groups["start"], mask, nhsNumberLast3Digits, match.Groups["end"]);
    }
}
