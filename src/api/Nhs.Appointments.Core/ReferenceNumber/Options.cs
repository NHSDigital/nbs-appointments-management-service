namespace Nhs.Appointments.Core.ReferenceNumber;

public class ReferenceNumberOptions
{
    public int HmacKeyVersion { get; set; }
    public byte[] HmacKey { get; set; }
}
