namespace Nhs.Appointments.Core
{
    public class ClinicalServiceStore : IClinicalServiceStore
    {
        private readonly IReadOnlyCollection<ClinicalServiceType> _serviceTypes = new List<ClinicalServiceType>()
        {
            new("RSV (Adult)", "RSV:Adult"),

            // Test Service types for Dev/Local

            // Test A
            new("Test A (Child)", "TestA:Child"),
            new("Test A (Adult)", "TestA:Adult"),
            new("Test A (Elder)", "TestA:Elder"),

            // Test B
            new("Test B (Child)", "TestB:Child"),
            new("Test B (Adult)", "TestB:Adult"),
            new("Test B (Elder)", "TestB:Elder"),

            // Test Co-Admin
            new("Test A/B (Child)", "TestATestB:Child"),
            new("Test A/B (Adult)", "TestATestB:Adult"),
            new("Test A/B (Elder)", "TestATestB:Elder")
        };

        public Task<IReadOnlyCollection<ClinicalServiceType>> Get() 
        {
            return Task.FromResult(this._serviceTypes);
        }
    }
}
