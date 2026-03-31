namespace Nhs.Appointments.Persistance;

public static class MetricScopes
{
    public static class Booking
    {
        public const string GetBookingsInDateRange = "GetBookingsInDateRange";
        public const string GetBookingsByFilter = "GetBookingsByFilter";
        public const string GetCrossSiteBookings = "GetCrossSiteBookings";
        public const string GetBookingByReference = "GetBookingByReference";
        public const string GetBookingsByNhsNumber = "GetBookingsByNhsNumber";
        public const string UpdateBookingStatus = "UpdateBookingStatus";
        public const string UpdateBookingAvailabilityStatus = "UpdateBookingAvailabilityStatus";
        public const string ConfirmProvisionalBooking = "ConfirmProvisionalBooking";
        public const string ConfirmProvisionalBookings = "ConfirmProvisionalBookings";
        public const string SetReminderSent = "ReminderSent";
        public const string InsertBooking = "InsertBooking";
        public const string RemoveUnconfirmedProvisionalBookings = "RemoveUnconfirmedProvisionalBookings";
        public const string DeleteBooking = "DeleteBooking";
        public const string CancelAllBookingsInADay = "CancelAllBookingsInADay";
        public const string CancellationNotified = "CancellationNotified";
        public const string GetRecentlyUpdatedBookingsCrossSite = "GetRecentlyUpdatedBookingsCrossSite";
        public const string GetBookingsInStatusUpdatedDateRange = "GetBookingsInStatusUpdatedDateRange";
    }

    public static class ClinicalService
    {
        public const string Get = "Get";
    }

    public static class DailySiteSummary
    {
        public const string Create = "Create";
        public const string GetSiteSummaries = "GetSiteSummaries";
        public const string DeleteSummary = "DeleteSummary";
    }

    public static class EmailWhiteList
    {
        public const string Get = "Get";
    }

    public static class Eula
    {
        public const string GetLatest = "GetLatest";
    }

    public static class NotificationConfiguration
    {
        public const string Get = "Get";
    }

    public static class ReferenceGroup
    {
        public const string Assign = "Assign";
        public const string GetNextSequenceNumberForPrefix = "GetNextSequenceNumberForPrefix";
    }

    public static class Roles
    {
        public const string Get = "Get";
    }

    public static class Sites
    {
        public const string GetById = "GetById";
        public const string GetAll = "GetAll";
        public const string GetReferenceNumberGroup = "GetReferenceNumberGroup";
        public const string GetSitesInRegion = "GetSitesInRegion ";
        public const string GetSitesInIcb = "GetSitesInIcb";
        public const string AssignPrefix = "AssignPrefix";
        public const string UpdateSiteReferenceDetails = "UpdateSiteReferenceDetails";
        public const string UpdateAccessibilityInfo = "UpdateAccessibilityInfo";
        public const string UpdateInformationForCitizens = "UpdateInformationForCitizens";
        public const string UpdateSiteDetails = "UpdateSiteDetails";
        public const string UpdateSiteStatus = "UpdateSiteStatus";
        public const string Save = "Save";
        public const string ToggleSiteSoftDeletion = "ToggleSiteSoftDeletion";
    }

    public static class Users
    {
        public const string GetUsersWithPermissionScope = "GetUsersWithPermissionScope";
        public const string GetApiUserSigningKey = "GetApiUserSigningKey";
        public const string GetUserById = "GetUserById";
        public const string GetUserRoleAssignments = "GetUserRoleAssignments";
        public const string UpdateUserRoleAssignments = "UpdateUserRoleAssignments";
        public const string RemoveUserByIdFromSite = "RemoveUserByIdFromSite";
        public const string UpdateUserRegionPermissions = "UpdateUserRegionPermissions";
        public const string RecordEulaAgreement = "RecordEulaAgreement";
        public const string GetUsersForSite = "GetUsersForSite";
        public const string SaveAdminUser = "SaveAdminUser";
        public const string RemoveAdminUser = "RemoveAdminUser";
        public const string UpdateUserIcbPermissions = "UpdateUserIcbPermissions";
        public const string InsertUser = "InsertUser";
    }

    public static class WellKnownOdsCodes
    {
        public const string Get = "Get";
    }

    public static class Audit
    {
        public const string RecordFunction = "RecordFunction";
        public const string RecordAuth = "RecordAuth";
        public const string RecordNotification = "RecordNotification";
        public const string RecordUserDeleted = "RecordUserDeleted";
    }

    public static class AccessibilityDefinitions
    {
        public const string Get = "Get";
    }

    public static class Aggregation
    {
        public const string GetLastRun = "GetLastRun";
        public const string SetLastRun = "SetLastRun";
    }

    public static class Availability
    {
        public const string ApplyAvailabilityTemplate = "ApplyAvailabilityTemplate";
        public const string CancelDayForSite = "CancelDayForSite";
        public const string CancelAllSessionsInDateRange = "CancelAllSessionsInDateRange";
        public const string CancelSession = "CancelSession";
        public const string CancelMultipleSessions = "CancelMultipleSessions";
        public const string EditSessions = "EditSessions";
        public const string GetAvailabilityCreatedEvents = "GetAvailabilityCreatedEvents";
        public const string GetDailyAvailability = "GetDailyAvailability";
        public const string GetSessions = "GetSessions";
        public const string SiteSupportsAllServicesOnSingleDateInRange = "SiteSupportsAllServicesOnSingleDateInRange";
    }
}
