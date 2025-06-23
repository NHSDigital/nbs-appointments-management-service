using System.Collections.Generic;

namespace Nhs.Appointments.Api.Notifications;
public class VaccineServiceHelper : IVaccineServiceHelper
{
    private readonly Dictionary<string, ServiceInfo> services = new()
    {
        { "COVID:5_11", new ServiceInfo { Vaccine = "COVID-19", Url = "https://www.nhs.uk/bookcovid" } },
        { "COVID:12_17", new ServiceInfo { Vaccine = "COVID-19", Url = "https://www.nhs.uk/bookcovid" } },
        { "COVID:18+", new ServiceInfo { Vaccine = "COVID-19", Url = "https://www.nhs.uk/bookcovid" } },
        { "Flu:2_3", new ServiceInfo { Vaccine = "flu", Url = "https://www.nhs.uk/bookflu" } },
        { "Flu_18_64", new ServiceInfo { Vaccine = "flu", Url = "https://www.nhs.uk/bookflu" } },
        { "Flu_65+", new ServiceInfo { Vaccine = "flu", Url = "https://www.nhs.uk/bookflu" } },
        { "COVID_FLU:18_64", new ServiceInfo { Vaccine = "COVID-19 and flu", Url = "https://www.nhs.uk/get-vaccination" } },
        { "COVID_FLU:65+", new ServiceInfo { Vaccine = "COVID-19 and flu", Url = "https://www.nhs.uk/get-vaccination" } },
        { "RSV:Adult", new ServiceInfo { Vaccine = "RSV", Url = "https://www.nhs.uk/book-rsv" } },
    };

    public string GetVaccineType(string service)
    {
        if (services.TryGetValue(service, out var info))
        {
            return info.Vaccine;
        }
        return null;
    }

    public string GetServiceUrl(string service)
    {
        if (services.TryGetValue(service, out var info))
        {
            return info.Url;
        }
        return null;
    }
}
