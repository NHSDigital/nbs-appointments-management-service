namespace Nhs.Appointments.Core.ReferenceNumber.V2;

public class ReferenceNumberOptions
{
    public int HmacKeyVersion { get; set; }
    public byte[] HmacKey { get; set; }
}
