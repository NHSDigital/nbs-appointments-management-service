using Parquet.Serialization;

namespace JobRunner.Job.Notify;

public class BookingInfoReader : INotifyInfoReader<BookingInfo>
{
    public async Task<IEnumerable<BookingInfo>> ReadStreamAsync(Stream stream)
    {
        return await ParquetSerializer.DeserializeAsync<BookingInfo>(stream);
    }
}

public class BookingInfo
{
    public string BOOKING_REFERENCE_NUMBER { get; set; }
    public string FIRST_NAME { get; set; }
    public string PHONE_NUMBER { get; set; }
    public string EMAIL { get; set; }
}
