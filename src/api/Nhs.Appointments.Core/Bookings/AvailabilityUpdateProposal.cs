namespace Nhs.Appointments.Core.Bookings;

public class AvailabilityUpdateProposal
{
    public AvailabilityUpdateProposal() { }
    public AvailabilityUpdateProposal(bool matchingSessionNotFound)
    {
        MatchingSessionNotFound = matchingSessionNotFound;
    }
    public int NewlySupportedBookingsCount { get; set; }
    public int NewlyUnsupportedBookingsCount { get; set; }
    public bool MatchingSessionNotFound { get; set; }
}
