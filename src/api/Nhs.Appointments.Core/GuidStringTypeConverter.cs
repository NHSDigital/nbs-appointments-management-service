using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;

namespace Nhs.Appointments.Core;

public class GuidStringTypeConverter : DefaultTypeConverter
{
    /// <summary>
    /// Custom TypeConverter to validate a string GUID
    /// </summary>
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
