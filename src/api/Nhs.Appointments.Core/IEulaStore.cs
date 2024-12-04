namespace Nhs.Appointments.Core;

public interface IEulaStore
{
    Task<EulaVersion> GetLatestVersion();
}
