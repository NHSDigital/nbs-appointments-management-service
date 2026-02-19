using Moq;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Audit.Services;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Audit.UnitTests.Services;

public class AuditWriteServiceTests
{
    private readonly AuditWriteService _sut;
    private readonly Mock<ITypedDocumentCosmosStore<AuditFunctionDocument>> _auditFunctionStore = new();
    private readonly Mock<ITypedDocumentCosmosStore<AuditAuthDocument>> _auditAuthStore = new();
    private readonly Mock<ITypedDocumentCosmosStore<AuditNotificationDocument>> _auditNotificationStore = new();
    private readonly Mock<ITypedDocumentCosmosStore<AuditUserRemovedDocument>> _auditUserRemovedStore = new();

    public AuditWriteServiceTests()
    {
        _sut = new AuditWriteService(
            _auditFunctionStore.Object,
            _auditAuthStore.Object,
            _auditNotificationStore.Object,
            _auditUserRemovedStore.Object
        );
    }

    [Fact]
    public async Task RecordUserDeleted()
    {
        //Arrange
        var userId = "test@okta.com";
        var scope = "scope:123";
        var removedBy = "dev@mail.com";
        var docType = "testingType";

        _auditUserRemovedStore.Setup(x => x.GetDocumentType()).Returns(docType);

        //Act
        await _sut.RecordUserDeleted(userId, scope, removedBy);

        //Assert
        _auditUserRemovedStore.Verify(x => x.GetDocumentType(), Times.Once);
        _auditUserRemovedStore.Verify(x => x.WriteAsync(It.Is<AuditUserRemovedDocument>(
            x => x.Scope == scope &&
            x.User == removedBy &&
            x.RemovedUser == userId &&
            x.DocumentType == docType
        )), Times.Once);
    }
}
