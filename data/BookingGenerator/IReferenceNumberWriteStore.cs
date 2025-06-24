using Nhs.Appointments.Core;

namespace BookingGenerator;

public interface IReferenceNumberWriteStore : IReferenceNumberDocumentStore
{
    Task SaveReferenceGroup();
}
