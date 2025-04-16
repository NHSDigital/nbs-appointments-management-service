namespace Nhs.Appointments.Core.UnitTests
{
    public class ClinicalServiceStoreTests
    {
        private readonly ClinicalServiceStore _sut;

        public ClinicalServiceStoreTests()
        {
            _sut = new ClinicalServiceStore();
        }

        // This test is to ensure we never accidentally add a duplicate service
        [Fact]
        public async Task Get_Value_AllUnique() 
        {
            var services = await _sut.Get();

            Assert.Distinct(services.Select(x => x.Value));
        }
    }
}
