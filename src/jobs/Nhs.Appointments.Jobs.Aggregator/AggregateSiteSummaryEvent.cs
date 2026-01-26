namespace Nhs.Appointments.Jobs.Aggregator;

public record AggregateSiteSummaryEvent(string Site, DateOnly Date);
