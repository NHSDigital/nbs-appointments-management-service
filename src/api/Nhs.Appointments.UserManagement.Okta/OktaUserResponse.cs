namespace Nhs.Appointments.UserManagement.Okta;
public class OktaUserResponse
{
    public bool IsProvisioned { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset Created { get; set; }
}
