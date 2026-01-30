using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Jobs.Aggregator;

public class LastUpdatedByResolver(
    string applicationName
) : ILastUpdatedByResolver
{
    public string GetLastUpdatedBy(){
        return applicationName;
    }
}
