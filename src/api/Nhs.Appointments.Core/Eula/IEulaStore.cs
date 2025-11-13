namespace Nhs.Appointments.Core.Eula;

public interface IEulaStore
{
    Task<EulaVersion> GetLatestVersion();
}
