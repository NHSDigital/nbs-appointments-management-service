using AutoMapper;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests
{
    public class UserStoreTests
    {
        private readonly Mock<ITypedDocumentCosmosStore<UserDocument>> _cosmosStore = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly UserStore _sut;

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
            _cosmosStore.Setup(x => x.ConvertToDocument(It.IsAny<Core.User>())).Returns(default(UserDocument));
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");
            _cosmosStore.Setup(x => x.WriteAsync(It.IsAny<UserDocument>()));

            var roleAssignments = new List<Core.RoleAssignment>() { new() { Role = "test", Scope = "*" } };

            await _sut.UpdateUserRoleAssignments("test", "*", roleAssignments);

            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<Core.User>(It.IsAny<string>()), Times.Once);
            _cosmosStore.Verify(x => x.ConvertToDocument(It.IsAny<Core.User>()), Times.Once);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");
            _cosmosStore.Verify(x => x.WriteAsync(It.IsAny<UserDocument>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserRoleAssignments_WhenAddRoles_UpdatesUser()
        {
            var user = new Core.User()
            {
                Id = "test",
                RoleAssignments = [new() { Role = "test", Scope = "*" }]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");
            _cosmosStore.Setup(x => x.PatchDocument(It.Is<string>(d => d.Equals("user")), It.IsAny<string>(), It.IsAny<PatchOperation[]>())).ReturnsAsync(default(UserDocument));

            var roleAssignments = new List<Core.RoleAssignment>() { new() { Role = "test", Scope = "*" }, new() { Role = "test-2", Scope = "*" } };

            await _sut.UpdateUserRoleAssignments("test", "*", roleAssignments);

            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<Core.User>(It.IsAny<string>()), Times.Once);
            _cosmosStore.Verify(x => x.PatchDocument(It.Is<string>(d => d.Equals("user")), It.IsAny<string>(), It.IsAny<PatchOperation[]>()));
        }

        [Fact]
        public async Task UpdateUserRegionPermissions_UpdatesUserDocument()
        {
            var user = new Core.User
            {
                Id = "test.user@nhs.net",
                RoleAssignments = [new() { Role = "system:regional-user", Scope = "region:Test" }]
            };

            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.User>(It.IsAny<string>())).ReturnsAsync(user);
            _cosmosStore.Setup(x => x.GetDocumentType()).Returns("user");

            await _sut.UpdateUserRegionPermissionsAsync("test.user@nhs.net", "region:Test", [new() { Role = "system:regional-user", Scope = "region:Test" }]);

            _cosmosStore.Verify(x => x.GetByIdOrDefaultAsync<Core.User>("test.user@nhs.net"), Times.Once);
            _cosmosStore.Verify(x => x.PatchDocument(It.IsAny<string>(), "test.user@nhs.net", It.IsAny<PatchOperation[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserRegionPermissions_AddNewUser()
        {
            _cosmosStore.Setup(x => x.GetByIdOrDefaultAsync<Core.User>(It.IsAny<string>()))
                .ReturnsAsync(null as Core.User);

            await _sut.UpdateUserRegionPermissionsAsync("test.user@nhs.net", "region:Test", [new() { Role = "system:regional-user", Scope = "region:Test" }]);

            _cosmosStore.Verify(x => x.WriteAsync(It.IsAny<UserDocument>()), Times.Once);
            _cosmosStore.Verify(x => x.PatchDocument(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PatchOperation[]>()), Times.Never);
        }
    }
}
