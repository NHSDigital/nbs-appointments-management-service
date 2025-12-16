namespace Nhs.Appointments.Core.Availability
{
    public interface IHasConsecutiveCapacityFilter
    {
        Task<SessionInstance[]> SessionHasConsecutiveSessions(SessionInstance[] slots, int consecutive);
    }
}
