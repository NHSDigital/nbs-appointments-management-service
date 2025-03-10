using Nbs.Appointments.Calculator.Models;

namespace Nbs.Appointments.Calculator;
public interface IAvailibilityCalculator
{
    int GetCapacityForService(string service, IEnumerable<Availability> availabilities, string[] bookedServices);
    Dictionary<string, int> GetCapacity(IEnumerable<Availability> availabilities, string[] bookedServices);
    IEnumerable<UnAvailable> GetNotAvailableCapacity(IEnumerable<Availability> availabilities, string[] bookedServices);
    Capacity GetCapacityReport(IEnumerable<Availability> availabilities, string[] bookedServices);
}
