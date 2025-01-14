namespace Nhs.Appointments.Audit.Models;

public record Audit(DateTime Timestamp, string UserId, string ActionType, string? SiteId);
