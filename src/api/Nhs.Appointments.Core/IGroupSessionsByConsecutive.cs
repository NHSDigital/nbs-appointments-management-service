namespace Nhs.Appointments.Core
{
    public interface IGroupSessionsByConsecutive
    {
        IEnumerable<SessionInstance> GroupByConsecutive(IEnumerable<SessionInstance> slots, int consecutive);
    }
}
