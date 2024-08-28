using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Notify.Interfaces;

namespace Nhs.Appointments.Api.Tests.Notifications
{
    public class UserRolesChangedNotifierTests
    {
        private UserRolesChangedNotifier _sut;
        private Mock<IAsyncNotificationClient> _notificationClient = new();
        private const string TemplateId = "my-template";
        private const string Email = "test@user";
        public UserRolesChangedNotifierTests()
        {
            var opts = new Mock<IOptions<UserRolesChangedNotifier.Options>>();
            opts.SetupGet(x => x.Value).Returns(new UserRolesChangedNotifier.Options { EmailTemplateId = TemplateId});
            _sut = new UserRolesChangedNotifier(_notificationClient.Object, opts.Object);
        }

        [Fact]
        public async Task PassesValuesToGovNotifyService()
        {
            _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => dic.ContainsKey("rolesAdded") && dic.ContainsKey("rolesRemoved")), null, null, null)).Verifiable();

            await _sut.Notify(Email, ["newRole"], ["removedRole"]);
            _notificationClient.Verify();
        }
    }
}
