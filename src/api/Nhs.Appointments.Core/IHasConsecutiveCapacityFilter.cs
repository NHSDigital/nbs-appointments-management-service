namespace Nhs.Appointments.Core
{
    public interface IHasConsecutiveCapacityFilter
    {
        IEnumerable<SessionInstance> SessionHasConsecutiveSessions(IEnumerable<SessionInstance> slots, int consecutive);
    }
}
