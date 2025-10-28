namespace Nhs.Appointments.Core.ReferenceNumber;

public abstract class ReferenceNumberOptions
{
    public int HmacKeyVersion { get; set; }
    public byte[] HmacKey { get; set; }
}
