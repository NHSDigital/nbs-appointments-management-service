namespace Nhs.Appointments.Core.Availability
{
    public interface IHasConsecutiveCapacityFilter
    {
        Task<HashSet<SessionInstance>> SessionHasConsecutiveSessions(HashSet<SessionInstance> slots, int consecutive);
    }
}
