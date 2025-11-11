namespace Nhs.Appointments.Core.Availability
{
    public interface IHasConsecutiveCapacityFilter
    {
        IEnumerable<SessionInstance> SessionHasConsecutiveSessions(IEnumerable<SessionInstance> slots, int consecutive);
    }
}
