namespace Nhs.Appointments.Api.Auth;

/// <summary>
/// Known roles used for authentication in the system
/// </summary>
public static class Roles
{
    public const string SystemAdminUser = "system:admin-user";
    public const string SystemRunReminders = "system:run-reminders";
    public const string SystemProvisionalSweeper = "system:run-provisional-sweep";
    
    public const string SiteManager = "site:manage";
    public const string SiteViewer = "site:view";
    public const string SiteMetadataViewer = "site:get-meta-data";
    public const string SiteQuery = "sites:query";
    
    public const string AvailabilitySetup = "availability:setup";
    public const string AvailabilityQuery = "availability:query";
    
    public const string MakeBooking = "booking:make";
    public const string QueryBooking = "booking:query";
    public const string CancelBooking = "booking:cancel";
    public const string SetBookingStatus = "booking:set-status";
    
    public const string ViewUsers = "users:view";
    public const string UserManager = "users:manage";
}
