using Nhs.Appointments.Core;

namespace BookingGenerator;

internal interface IReferenceNumberWriteStore : IReferenceNumberDocumentStore
{
    Task SaveReferenceGroup();
}
