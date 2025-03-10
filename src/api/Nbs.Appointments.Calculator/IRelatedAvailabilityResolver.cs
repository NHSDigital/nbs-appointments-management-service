using Nbs.Appointments.Calculator.Models;

namespace Nbs.Appointments.Calculator;
public interface IRelatedAvailabilityResolver
{
    IEnumerable<string> ResolveRelatedAvailabilities(string[] availability, IEnumerable<Availability> availabilities);
}
