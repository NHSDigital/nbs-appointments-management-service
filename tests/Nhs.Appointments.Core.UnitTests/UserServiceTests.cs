using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;
using Nhs.Appointments.Core.Okta;
using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Core.UnitTests
{
    public class UserServiceTests
    {
        private readonly UserService _sut;
        private readonly Mock<IUserStore> _userStore = new();
        private readonly Mock<IMessageBus> _messageBus = new();
        private readonly Mock<IRolesStore> _rolesStore = new();
        private readonly Mock<IOktaUserDirectory> _oktaUserDirectory = new();
        private readonly Mock<IEmailWhitelistStore> _emailWhitelistStore = new();

        private readonly List<string> whiteListedEmails =
            ["@nhs.net", "@not-nhs.net", "not-nhs-either.net", "@trailing-space.net "];


        public UserServiceTests()
        {
            _emailWhitelistStore.Setup(emailWhitelistStore => emailWhitelistStore.GetWhitelistedEmails())
                .ReturnsAsync(whiteListedEmails);
            
            _sut = new UserService(
                _userStore.Object,
                _rolesStore.Object,
                _messageBus.Object,
                _oktaUserDirectory.Object,
                _emailWhitelistStore.Object
            );
        }

        private void UserExistsInMya()
        {
            _userStore
                .Setup(userStore => userStore.GetOrDefaultAsync(It.IsAny<string>())).ReturnsAsync(new User
                {
                    RoleAssignments =
                    [
                        new RoleAssignment { Scope = "site:some-site" }
                    ]
                });
        }

        private void UserDoesNotExistInMya()
        {
            _userStore
                .Setup(userStore => userStore.GetOrDefaultAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
        }

        private void UserExistsInOkta()
        {
            _oktaUserDirectory.Setup(oktaUserDirectory => oktaUserDirectory.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync(new OktaUserResponse());
        }

        private void UserDoesNotExistInOkta()
        {
            _oktaUserDirectory.Setup(oktaUserDirectory => oktaUserDirectory.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync((OktaUserResponse)null);
        }

        [Fact]
        public async Task RaisesEventWhenRolesAreAdded()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole", Scope = scope }]));
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
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>()));
            _userStore.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new User { Id = userId, RoleAssignments = [new RoleAssignment { Role = "someoldrole", Scope = scope }] });
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
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole", Scope = scope }]));
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
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole", Scope = scope }]));
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
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }, new RoleAssignment { Role = "not a role", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }, new Role { Id = "role2"}];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>())).Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole", Scope = scope }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));

            var result = await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            Assert.False(result.Success);
            Assert.Equal("not a role", result.ErrorRoles[0]);
        }

        [Fact]
        public async Task RaisesEventWithNewRoles_WhenUserIsAddedForTheFirstTime()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>()));
            _userStore.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync((User)null);
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
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.UpdateUserRoleAssignments(It.IsAny<string>(), scope, It.IsAny<IEnumerable<RoleAssignment>>()));
            _userStore.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new User() { Id = userId, RoleAssignments = [new RoleAssignment { Role = "someoldrole", Scope = scope }] });
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));

            _ = await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            var expectedUserId = userId.ToLower();

            _messageBus.Verify(x => x.Send(It.Is<UserRolesChanged>(e => e.UserId.Equals(expectedUserId))));
            _userStore.Verify(x => x.UpdateUserRoleAssignments(expectedUserId, scope, It.IsAny<IEnumerable<RoleAssignment>>()), Times.Once);
        }

        [Fact]
        public async Task WhenMultipleScopes_ShouldThrowInvalidException()
        {
            const string userId = "test@nhs.net";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = "site:some-other-site" }];

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles));
        }

        [Fact]
        public async Task WhenUserGainsRole_AndHasAnotherScope_NotificationOnlySendForThatScope()
        {
            const string userId = "test@nhs.net";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.UpdateUserRoleAssignments(It.IsAny<string>(), scope, It.IsAny<IEnumerable<RoleAssignment>>()));
            _userStore.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new User() { Id = userId, RoleAssignments = [new RoleAssignment { Role = "role1", Scope = "someotherscope" }] });
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));

            _ = await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify(x => x.Send(It.Is<UserRolesChanged>(e => e.AddedRoleIds.Contains("role1") && e.AddedRoleIds.Length == 1 && e.RemovedRoleIds.Length == 0)));
            _userStore.Verify(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>()), Times.Once);
        }

        [Fact]
        public async Task WhenUserLosesRole_AndHasAnotherScope_NotificationOnlySendForThatScope()
        {
            const string userId = "test@nhs.net";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.UpdateUserRoleAssignments(It.IsAny<string>(), scope, It.IsAny<IEnumerable<RoleAssignment>>()));
            _userStore.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(new User() { Id = userId, RoleAssignments = [new RoleAssignment { Role = "role1", Scope = scope }, new RoleAssignment { Role = "role1", Scope = "someotherscope" }] });
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));

            _ = await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles);

            _messageBus.Verify(x => x.Send(It.Is<UserRolesChanged>(e => e.RemovedRoleIds.Contains("role1") && e.RemovedRoleIds.Length == 1 && e.AddedRoleIds.Length == 0)));
            _userStore.Verify(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>()), Times.Once);
        }

        [Theory]
        [InlineData("TEST.USER1@NHS.NET")]
        [InlineData("TeSt.uSeR2@nhs.NET")]
        [InlineData("test.User3@nHs.NeT")]
        public async Task GetUserIdentityStatus_NhsUser_WhenUserDoesNotExistInMya(string userId)
        {
            UserDoesNotExistInMya();

            var identityStatus = await _sut.GetUserIdentityStatusAsync("some-site", userId);

            identityStatus.IdentityProvider.Should().Be(IdentityProvider.NhsMail);
            identityStatus.ExtantInSite.Should().BeFalse();
            // We currently assume all @nhs.net email addresses are valid
            identityStatus.ExtantInIdentityProvider.Should().BeTrue();
            identityStatus.MeetsWhitelistRequirements.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserIdentityStatus_NhsUser_WhenUserDoesExistInMya()
        {
            UserExistsInMya();

            var identityStatus = await _sut.GetUserIdentityStatusAsync("some-site", "some.user@nhs.net");

            identityStatus.IdentityProvider.Should().Be(IdentityProvider.NhsMail);
            identityStatus.ExtantInSite.Should().BeTrue();
            // We currently assume all @nhs.net email addresses are valid
            identityStatus.ExtantInIdentityProvider.Should().BeTrue();
            identityStatus.MeetsWhitelistRequirements.Should().BeTrue();
        }

        [Theory]
        [InlineData("OKTA.USER1@not-nhs.NET")]
        [InlineData("OkTa.uSeR2@not-nhs-either.NET")]
        [InlineData("okta.user3@trailing-space.net ")]
        public async Task GetUserIdentityStatus_Okta_User_WhenUserDoesNotExistInMya_AndDoesNotExistInOkta(string userId)
        {
            UserDoesNotExistInMya();
            UserDoesNotExistInOkta();

            var identityStatus = await _sut.GetUserIdentityStatusAsync("some-site", userId);

            identityStatus.IdentityProvider.Should().Be(IdentityProvider.Okta);
            identityStatus.ExtantInSite.Should().BeFalse();
            identityStatus.ExtantInIdentityProvider.Should().BeFalse();
            identityStatus.MeetsWhitelistRequirements.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserIdentityStatus_Okta_User_WhenUserDoesExistInMya_AndDoesNotExistInOkta()
        {
            UserExistsInMya();
            UserDoesNotExistInOkta();

            var identityStatus = await _sut.GetUserIdentityStatusAsync("some-site", "some.user@not-nhs.net");

            identityStatus.IdentityProvider.Should().Be(IdentityProvider.Okta);
            identityStatus.ExtantInSite.Should().BeTrue();
            identityStatus.ExtantInIdentityProvider.Should().BeFalse();
            identityStatus.MeetsWhitelistRequirements.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserIdentityStatus_Okta_User_WhenUserDoesNotExistInMya_AndDoesExistInOkta()
        {
            UserDoesNotExistInMya();
            UserExistsInOkta();

            var identityStatus = await _sut.GetUserIdentityStatusAsync("some-site", "some.user@not-nhs.net");

            identityStatus.IdentityProvider.Should().Be(IdentityProvider.Okta);
            identityStatus.ExtantInSite.Should().BeFalse();
            identityStatus.ExtantInIdentityProvider.Should().BeTrue();
            identityStatus.MeetsWhitelistRequirements.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserIdentityStatus_Okta_User_WhenUserDoesExistInMya_AndDoesExistInOkta()
        {
            UserExistsInMya();
            UserExistsInOkta();

            var identityStatus = await _sut.GetUserIdentityStatusAsync("some-site", "some.user@not-nhs.net");

            identityStatus.IdentityProvider.Should().Be(IdentityProvider.Okta);
            identityStatus.ExtantInSite.Should().BeTrue();
            identityStatus.ExtantInIdentityProvider.Should().BeTrue();
            identityStatus.MeetsWhitelistRequirements.Should().BeTrue();
        }


        [Fact]
        public async Task GetUserIdentityStatus_Okta_User_WhitelistRequirementsNotMet()
        {
            UserExistsInMya();
            UserExistsInOkta();

            var identityStatus = await _sut.GetUserIdentityStatusAsync("some-site", "some.user@not-a-valid-domain.net");

            identityStatus.IdentityProvider.Should().Be(IdentityProvider.Okta);
            identityStatus.ExtantInSite.Should().BeTrue();
            identityStatus.ExtantInIdentityProvider.Should().BeTrue();
            identityStatus.MeetsWhitelistRequirements.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserIdentityStatus_WhenUserDoesExistInMyaButNotInGivenSite()
        {
            UserExistsInOkta();

            _userStore
                .Setup(userStore => userStore.GetOrDefaultAsync(It.IsAny<string>())).ReturnsAsync(new User
                {
                    RoleAssignments =
                    [
                        new RoleAssignment { Scope = "site:some-OTHER-site" }
                    ]
                });

            var identityStatus = await _sut.GetUserIdentityStatusAsync("some-site", "some.user@not-nhs.net");

            identityStatus.IdentityProvider.Should().Be(IdentityProvider.Okta);
            identityStatus.ExtantInSite.Should().BeFalse();
            identityStatus.ExtantInIdentityProvider.Should().BeTrue();
            identityStatus.MeetsWhitelistRequirements.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateUserRoles_DoesNotCallMessageBus_WhenSendNotificationsIsFalse()
        {
            const string userId = "user1";
            const string scope = "site:some-site";
            RoleAssignment[] newRoles = [new RoleAssignment { Role = "role1", Scope = scope }];
            IEnumerable<Role> databaseRoles = [new Role { Id = "role1" }];

            _userStore.Setup(x => x.GetOrDefaultAsync(userId)).Returns(Task.FromResult<User>(new User { Id = userId }));
            _userStore.Setup(x => x.UpdateUserRoleAssignments(userId, scope, It.IsAny<IEnumerable<RoleAssignment>>()))
                .Returns(Task.FromResult<RoleAssignment[]>([new RoleAssignment { Role = "someoldrole", Scope = scope }]));
            _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult(databaseRoles));
            _messageBus.Setup(x => x.Send(It.Is<UserRolesChanged>(e => e.AddedRoleIds.Contains(newRoles[0].Role)))).Verifiable();

            await _sut.UpdateUserRoleAssignmentsAsync(userId, scope, newRoles, false);

            _messageBus.Verify(x => x.Send(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task SaveAdminUser_AssignsAdminUserRole()
        {
            var adminEmail = "admin.user@nhs.net";
            var expectedUser = new User
            {
                Id = adminEmail,
                LatestAcceptedEulaVersion = null,
                RoleAssignments =
                [
                    new()
                    {
                        Role = "system:admin-user",
                        Scope = "global"
                    }
                ]
            };

            await _sut.SaveAdminUserAsync("admin.user@nhs.net");

            _userStore.Verify(x => x.SaveAdminUserAsync(It.Is<User>(
                u => u.Id == adminEmail &&
                     u.RoleAssignments.Length == 1 &&
                     u.RoleAssignments.First().Role == expectedUser.RoleAssignments.First().Role &&
                     u.RoleAssignments.First().Scope == expectedUser.RoleAssignments.First().Scope &&
                     u.LatestAcceptedEulaVersion == expectedUser.LatestAcceptedEulaVersion)), Times.Once);
        }
    }
}
