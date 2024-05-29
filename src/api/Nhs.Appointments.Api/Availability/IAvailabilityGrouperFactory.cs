namespace Nhs.Appointments.Api.Availability;

public interface IAvailabilityGrouperFactory
{
    IAvailabilityGrouper Create(QueryType queryType);
}