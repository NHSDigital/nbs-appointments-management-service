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
            _sut = new UserRolesChangedNotifier(_notificationClient.Object, TemplateId);
        }

        [Fact]
        public async Task SendsEmailUsingGovNotify()
        {
            _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.IsAny<Dictionary<string, dynamic>>(), null, null, null)).Verifiable();
            await _sut.Notify(Email, ["role1"]);
            _notificationClient.Verify();
        }
    }
}
