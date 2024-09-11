using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core.UnitTests
{
    public class UserServiceTests
    {
        private UserService _sut;
        private Mock<IUserStore> _userStore = new();
        private Mock<IMessageBus> _messageBus = new();
        private Mock<IRolesStore> _rolesStore = new();

        public UserServiceTests()
        {
            _sut = new(_userStore.Object, _rolesStore.Object, _messageBus.Object);
        }

        [Fact]
        public async void RaisesEventWhenRolesAreAdded()
        {
            string userId = "user1";
            string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole"}]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.AddedRoleIds.Contains(newRoles[0].Role)))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async void RaisesEventWhenRolesAreRemoved()
        {
            string userId = "user1";
            string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.RemovedRoleIds.Contains("someoldrole")))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async void IncludesSiteIdInEvent()
        {
            string userId = "user1";
            string scope = "site:some-site";
            string site = "some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.SiteId == site))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async void IncludesUserIdInEvent()
        {
            string userId = "user1";
            string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.UserId == userId))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async void ReturnsFailureWhenUserNotFound()
        {
            string userId = "user1";
            string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(null));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));

            var result = await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            Assert.False(result.Success);
            Assert.Equal(userId, result.ErrorUser);
        }

        [Fact]
        public async void ReturnsFailureWhenRoleNotFound()
        {
            string userId = "user1";
            string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }, new RoleAssignment { Role = "not a role"}];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }, new Role { Id = "role2"}];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));

            var result = await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            Assert.False(result.Success);
            Assert.Equal("not a role", result.ErrorRoles[0]);
        }


    }
}
