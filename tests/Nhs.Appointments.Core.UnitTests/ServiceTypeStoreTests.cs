namespace Nhs.Appointments.Core.UnitTests
{
    public class ServiceTypeStoreTests
    {
        private readonly ServiceTypeStore _sut;

        public ServiceTypeStoreTests()
        {
            _sut = new ServiceTypeStore();
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
