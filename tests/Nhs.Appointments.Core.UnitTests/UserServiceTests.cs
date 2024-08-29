using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core.UnitTests
{
    public class UserServiceTests
    {
        private UserService _sut;
        private Mock<IUserStore> _userStore = new();
        private Mock<IMessageBus> _messageBus = new();

        public UserServiceTests()
        {
            _sut = new(_userStore.Object,_messageBus.Object);
        }

        [Fact]
        public async void RaisesEventWhenRolesAreAdded()
        {
            string userId = "user1";
            string scope = "scope1";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];

            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole"}]));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.Added.Contains(newRoles[0].Role)))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }

        [Fact]
        public async void RaisesEventWhenRolesAreRemoved()
        {
            string userId = "user1";
            string scope = "scope1";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1" }];

            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole" }]));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.Removed.Contains("someoldrole")))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify();
        }
    }
}
