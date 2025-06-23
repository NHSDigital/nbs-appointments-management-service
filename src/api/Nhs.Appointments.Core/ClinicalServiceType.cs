namespace Nhs.Appointments.Core
{
    public record ClinicalServiceType
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public string ServiceType { get; set; }
        public string Url { get; set; }
    }
}
