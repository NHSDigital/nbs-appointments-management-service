namespace Nhs.Appointments.Core.Okta;

public class OktaUserResponse
{
    public DateTimeOffset Created { get; set; }

    public OktaUserStatus Status { get; set; }
}
