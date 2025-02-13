using System.Text;

namespace Nhs.Appointments.Core.UnitTests;
internal static class CsvFileBuilder
{
    public static string BuildInputCsv(string header, IEnumerable<string> dataLines)
    {
        var result = new StringBuilder(header);
        result.Append("\r\n");
        foreach (var line in dataLines)
        {
            result.Append($"{line}\r\n");
        }

        return result.ToString();
    }
}
