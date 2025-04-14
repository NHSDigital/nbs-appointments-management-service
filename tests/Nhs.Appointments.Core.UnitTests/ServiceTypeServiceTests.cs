using Nhs.Appointments.Core.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhs.Appointments.Core.UnitTests
{
    public class ServiceTypeServiceTests
    {
        private readonly ServiceTypeService _sut;
        private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
        private readonly Mock<IServiceTypeStore> _serviceTypeStore = new();

        private readonly IReadOnlyCollection<ServiceType> _serviceTypes = new List<ServiceType>()
        {
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

        public ServiceTypeServiceTests()
        {
            _sut = new ServiceTypeService(
                _serviceTypeStore.Object,
                _featureToggleHelper.Object);
        }

        [Fact]
        public async Task Get_ReturnsAll_WhenEnabled() 
        {
            _serviceTypeStore.Setup(x => x.Get()).ReturnsAsync(_serviceTypes);
            _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _sut.Get();

            Assert.Equivalent(_serviceTypes, result);
            _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.IsAny<string>()), Times.Exactly(9));
        }

        [Fact]
        public async Task Get_ReturnsNone_WhenDisabled()
        {
            _serviceTypeStore.Setup(x => x.Get()).ReturnsAsync(_serviceTypes);
            _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(false);

            var result = await _sut.Get();

            Assert.Empty(result);
            _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.IsAny<string>()), Times.Exactly(9));
        }

        [Fact]
        public async Task Get_ReturnsSome_WhenSomeEnabled()
        {
            _serviceTypeStore.Setup(x => x.Get()).ReturnsAsync(_serviceTypes);
            _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(x => x.Equals("TestA:Child")))).ReturnsAsync(false);
            _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(x => !x.Equals("TestA:Child")))).ReturnsAsync(true);
            _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);

            var result = await _sut.Get();

            Assert.Equivalent(_serviceTypes, result);
            _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.Is<string>(x => x.Equals("TestA:Child"))), Times.Once);
            _featureToggleHelper.Verify(x => x.IsFeatureEnabled(It.Is<string>(x => !x.Equals("TestA:Child"))), Times.Exactly(8));
        }
    }
}
