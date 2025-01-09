namespace Nhs.Appointments.Audit.Models;

public record Audit(string EventId, DateTime Timestamp, string UserId, string ActionType, string? SiteId);
