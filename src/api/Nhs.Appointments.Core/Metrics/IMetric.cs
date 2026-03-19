namespace Nhs.Appointments.Core.Metrics;

/// <summary>
/// This empty interface forms the basis of all measures that are being logged.
/// </summary>
public interface IMetric
{
    string Name { get; }
}
