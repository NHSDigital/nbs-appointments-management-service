namespace Nhs.Appointments.Api.Auth;

/// <summary>
/// Known roles used for authentication in the system
/// </summary>
public static class Permissions
{
    public const string SystemRunReminders = "system:run-reminders";
    public const string SystemRunProvisionalSweeper = "system:run-provisional-sweep";
    public const string SystemDataImporter = "system:data-importer";
    public const string ManageSiteAdmin = "site:manage:admin";

    public const string ManageSite = "site:manage";
    public const string ViewSite = "site:view";
    public const string ViewSitePreview = "site:view:preview";
    public const string ViewSiteMetadata = "site:get-meta-data";
    public const string QuerySites = "sites:query";

    public const string SetupAvailability = "availability:setup";
    public const string QueryAvailability = "availability:query";

    public const string MakeBooking = "booking:make";
    public const string QueryBooking = "booking:query";
    public const string CancelBooking = "booking:cancel";
    public const string SetBookingStatus = "booking:set-status";

    public const string ViewUsers = "users:view";
    public const string ManageUsers = "users:manage";
    
    public const string ReportsSiteSummary = "reports:sitesummary";
}
