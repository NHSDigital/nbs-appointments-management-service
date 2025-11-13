using AutoMapper;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests
{
    public class UserStoreTests
    {
        private readonly Mock<ITypedDocumentCosmosStore<UserDocument>> _cosmosStore = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly UserStore _sut;

        private const string IcbRole = "system:icb-user";
        private const string RegionRole = "system:regional-user";

        public UserStoreTests()
        {
            _sut = new UserStore(
                _cosmosStore.Object,
                _mapper.Object
                );
        }

        [Fact]
        public async Task UpdateUserRoleAssignments_WhenNoUser_CreatesUser() 
        {
            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<User>(It.IsAny<string>())).ReturnsAsync((User)null);
            _cosmosStore.Setup(x => x.ConvertToDocument(It.IsAny<Core.Users.User>())).Returns(default(UserDocument));
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");
            _cosmosStore.Setup(x => x.WriteAsync(It.IsAny<UserDocument>()));

            var roleAssignments = new List<Core.Users.RoleAssignment>() { new() { Role = "test", Scope = "*" } };

            await _sut.UpdateUserRoleAssignments("test", "*", roleAssignments);

            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>()), Times.Once);
            _cosmosStore.Verify(x => x.ConvertToDocument(It.IsAny<Core.Users.User>()), Times.Once);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");
            _cosmosStore.Verify(x => x.WriteAsync(It.IsAny<UserDocument>()), Times.Once);
        }

        [Theory]
        [InlineData("abc@test.com", "abc@test.com")]
        [InlineData("abc@TEST.com", "abc@test.com")]
        [InlineData("ABC@test.com", "abc@test.com")]
        [InlineData("abc@test.COM", "abc@test.com")]
        [InlineData("ABC@TEST.COM", "abc@test.com")]
        public async Task GetUserRoleAssignments_WhenPassedUpperCase_ShouldPass_LowerCase(string email, string expected)
        {
            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<UserDocument>(expected)).ReturnsAsync(new UserDocument { Id = expected, RoleAssignments = new RoleAssignment[] { new() { Role = "test", Scope = "*" } } });
            await _sut.GetUserRoleAssignments(email);
            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<UserDocument>(expected), Times.Once);
        }
        
        [Theory]
        [InlineData("abc@test.com", "abc@test.com")]
        [InlineData("abc@TEST.com", "abc@test.com")]
        [InlineData("ABC@test.com", "abc@test.com")]
        [InlineData("abc@test.COM", "abc@test.com")]
        [InlineData("ABC@TEST.COM", "abc@test.com")]
        public async Task GetUserAsync_WhenPassedUpperCase_ShouldPass_LowerCase(string email, string expected)
        {
            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(expected)).ReturnsAsync(new Core.Users.User { Id = expected });
            await _sut.GetUserAsync(email);
            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<Core.Users.User>(expected), Times.Once);
        }

        [Fact]
        public async Task UpdateUserRoleAssignments_WhenAddRoles_UpdatesUser()
        {
            var user = new Core.Users.User()
            {
                Id = "test",
                RoleAssignments = [new() { Role = "test", Scope = "*" }]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");
            _cosmosStore.Setup(x => x.PatchDocument(It.Is<string>(d => d.Equals("user")), It.IsAny<string>(), It.IsAny<PatchOperation[]>())).ReturnsAsync(default(UserDocument));

            var roleAssignments = new List<Core.Users.RoleAssignment>() { new() { Role = "test", Scope = "*" }, new() { Role = "test-2", Scope = "*" } };

            await _sut.UpdateUserRoleAssignments("test", "*", roleAssignments);

            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>()), Times.Once);
            _cosmosStore.Verify(x => x.PatchDocument(It.Is<string>(d => d.Equals("user")), It.IsAny<string>(), It.IsAny<PatchOperation[]>()));
        }

        [Fact]
        public async Task UpdateUserRegionPermissions_UpdatesUserDocument()
        {
            var user = new Core.Users.User
            {
                Id = "test.user@nhs.net",
                RoleAssignments = [new() { Role = RegionRole, Scope = "region:Test" }]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");

            await _sut.UpdateUserRegionPermissionsAsync("test.user@nhs.net", "region:Test", [new() { Role = RegionRole, Scope = "region:Test" }]);

            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<Core.Users.User>("test.user@nhs.net"), Times.Once);
            _cosmosStore.Verify(x => x.PatchDocument(It.IsAny<string>(), "test.user@nhs.net", It.IsAny<PatchOperation[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserRegionPermissions_AddNewUser()
        {
            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>()))
                .ReturnsAsync(null as Core.Users.User);

            await _sut.UpdateUserRegionPermissionsAsync("test.user@nhs.net", "region:Test", [new() { Role = RegionRole, Scope = "region:Test" }]);

            _cosmosStore.Verify(x => x.WriteAsync(It.IsAny<UserDocument>()), Times.Once);
            _cosmosStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserIcbPermissions_AddsNewUser()
        {
            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>()))
                .ReturnsAsync(null as Core.Users.User);

            await _sut.UpdateUserIcbPermissionsAsync("test.user@nhs.net", "icb:ICB1", [new() { Role = "system:icb-user", Scope = "icb:ICB1" }]);

            _cosmosStore.Verify(x => x.WriteAsync(It.IsAny<UserDocument>()), Times.Once);
            _cosmosStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserIcbPermissions_UpdatesUserDocument_AndRemovesExistingIcbRole()
        {
            var user = new  Core.Users.User
            {
                Id = "test.user@nhs.net",
                RoleAssignments = [new() { Role = IcbRole, Scope = "icb:ICB1" }]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");

            await _sut.UpdateUserIcbPermissionsAsync("test.user@nhs.net", "icb:ICB1", [new() { Role = IcbRole, Scope = "icb:ICB1" }]);

            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<Core.Users.User>("test.user@nhs.net"), Times.Once);
            _cosmosStore.Verify(x => x.PatchDocument(It.IsAny<string>(), "test.user@nhs.net", It.IsAny<PatchOperation[]>()), Times.Once);
        }

        [Fact]
        public void ReturnsEmptyArrayForUpdatedPermissions_WhenUserAlreadyHasRole()
        {
            var roleAssigments = new List<Core.Users.RoleAssignment> { new() { Role = "system:icb-user", Scope = "icb:ICB1" } };
            var user = new Core.Users.User
            {
                Id = "test.user@nhs.net",
                RoleAssignments = [.. roleAssigments]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");

            var result = _sut.GetUpdatedPermissions(user.RoleAssignments, "icb:ICB1", [.. roleAssigments], IcbRole, RegionRole);

            result.Count().Should().Be(0);
        }

        [Fact]
        public void ReturnsUpdateRole_WhenRoleHasChangedScope()
        {
            var roleAssigments = new List<Core.Users.RoleAssignment> { new() { Role = IcbRole, Scope = "icb:ICB1" } };
            var newRoleAssignments = new List<Core.Users.RoleAssignment> { new() { Role = IcbRole, Scope = "icb:ICB2" } };
            var user = new Core.Users.User
            {
                Id = "test.user@nhs.net",
                RoleAssignments = [.. roleAssigments]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");

            var result = _sut.GetUpdatedPermissions(user.RoleAssignments, "icb:ICB2", newRoleAssignments, IcbRole, RegionRole);

            result.Count().Should().Be(1);
            result.First().Scope.Should().Be("icb:ICB2");
        }

        [Fact]
        public void AddsUpdatedRole()
        {
            var newRoleAssignments = new List<Core.Users.RoleAssignment> { new() { Role = IcbRole, Scope = "icb:ICB1" } };
            var user = new Core.Users.User
            {
                Id = "test.user@nhs.net",
                RoleAssignments = []
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");

            var result = _sut.GetUpdatedPermissions(user.RoleAssignments, "icb:ICB1", newRoleAssignments, IcbRole, RegionRole);

            result.Count().Should().Be(1);
            result.First().Scope.Should().Be("icb:ICB1");
        }

        [Fact]
        public void RemovesConflictingPermission()
        {
            var newRoleAssignments = new List<Core.Users.RoleAssignment> { new() { Role = IcbRole, Scope = "icb:ICB1" } };
            var user = new Core.Users.User
            {
                Id = "test.user@nhs.net",
                RoleAssignments = [new() { Role = RegionRole, Scope = "region:R1" }]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");

            var result = _sut.GetUpdatedPermissions(user.RoleAssignments, "icb:ICB1", newRoleAssignments, IcbRole, RegionRole);

            result.Count().Should().Be(1);
            result.First().Scope.Should().Be("icb:ICB1");
        }

        [Fact]
        public void DoesntRemoveConfictingPermission_IfRemovingScopedRole()
        {

            var newRoleAssignments = new List<Core.Users.RoleAssignment> { new() { Role = IcbRole, Scope = "icb:ICB1" } };
            var user = new Core.Users.User
            {
                Id = "test.user@nhs.net",
                RoleAssignments = [new() { Role = RegionRole, Scope = "region:R1" }, new() { Role = IcbRole, Scope = "icb:ICB1" }]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.Users.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");

            var result = _sut.GetUpdatedPermissions(user.RoleAssignments, "icb:ICB1", newRoleAssignments, IcbRole, RegionRole);

            result.Count().Should().Be(1);
            result.First().Scope.Should().Be("region:R1");
        }
    }
}
