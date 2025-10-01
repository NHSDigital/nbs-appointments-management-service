using Nhs.Appointments.Core;
using System;

namespace Nhs.Appointments.Api.Models;

public record AvailabilityChangeProposalRequest(
    string Site,
    DateOnly From,
    DateOnly To,
    SessionOrWildcard SessionMatcher,
    Session? SessionReplacement
) : BaseSessionRequest(Site, From, To, SessionMatcher, SessionReplacement);
