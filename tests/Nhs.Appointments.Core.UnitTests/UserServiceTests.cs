using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core.UnitTests
{
    public class UserServiceTests
    {
        private readonly UserService _sut;
        private readonly Mock<IUserStore> _userStore = new();
        private readonly Mock<IMessageBus> _messageBus = new();
        private readonly Mock<IRolesStore> _rolesStore = new();

        public UserServiceTests()
        {
            _sut = new(_userStore.Object, _rolesStore.Object, _messageBus.Object);
        }

        [Fact]
        public async Task RaisesEventWhenRolesAreAdded()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole"}]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.AddedRoleIds.Contains(newRoles[0].Role)))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async Task RaisesEventWhenRolesAreRemoved()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.RemovedRoleIds.Contains("someoldrole")))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async Task IncludesSiteIdInEvent()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            const string site = "some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.SiteId == site))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async Task IncludesUserIdInEvent()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.UserId == userId))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async Task ReturnsFailureWhenRoleNotFound()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }, new RoleAssignment { Role = "not a role"}];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }, new Role { Id = "role2"}];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));

            var result = await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            Assert.False(result.Success);
            Assert.Equal("not a role", result.ErrorRoles[0]);
        }

        [Fact]
        public async Task RasisesEventWithNewRoles_WhenUserIsAddedForTheFirstTime()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>()))
                .ReturnsAsync([]);
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.AddedRoleIds.Length == 1 && e.AddedRoleIds.First() == "role1" && e.RemovedRoleIds.Length == 0))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Theory]
        [InlineData("TEST.USER1@NHS.NET")]
        [InlineData("TeSt.uSeR2@nhs.NET")]
        [InlineData("test.User3@nHs.NeT")]
        public async Task ConvertsUserIdToLowerCaseWhenCallingStore(string userId)
        {
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.UpdateUserRoleAssignments(It.IsAny<string>(), scope, It.IsAny<IEnumerable<RoleAssignment>>()))
                .ReturnsAsync([new RoleAssignment { Role = "someoldrole" }]);
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));

            _ = await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            var expectedUserId = userId.ToLower();

            _messageBus.Verify(x => x.Send(It.Is<UserRolesChanged>(e => e.UserId.Equals(expectedUserId))));
            _userStore.Verify(x => x.UpdateUserRoleAssignments(expectedUserId, scope, It.IsAny<IEnumerable<RoleAssignment>>()), Times.Once);
        }
    }
}
