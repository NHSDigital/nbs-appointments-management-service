using Nhs.Appointments.Core.Features;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Collections;

public static class FeatureToggleCollectionNames
{
    public const string JointBookingsCollection = $"{Flags.JointBookings}Toggle";
    public const string SiteSummaryReportCollection = $"{Flags.SiteSummaryReport}Toggle";
    public const string SiteStatusCollection = $"{Flags.SiteStatus}Toggle";
    public const string CancelDayCollection = $"{Flags.CancelDay}Toggle";
    public const string ChangeSessionUpliftedJourneyCollection = $"{Flags.ChangeSessionUpliftedJourney}Toggle";
    public const string CancelSessionUpliftedJourneyCollection = $"{Flags.CancelSessionUpliftedJourney}Toggle";
    public const string QuerySitesCollection = $"{Flags.QuerySites}Toggle";
    public const string MultiServiceJointBookingsCollection = $"{Flags.MultiServiceJointBookings}Toggle";
    public const string ReportsUpliftCollection = $"{Flags.ReportsUplift}Toggle";
}

[CollectionDefinition(FeatureToggleCollectionNames.JointBookingsCollection)]
public class JointBookingsSerialToggleCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.SiteSummaryReportCollection)]
public class SiteSummaryReportSerialToggleCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.SiteStatusCollection)]
public class SiteStatusSerialToggleCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.CancelDayCollection)]
public class CancelDaySerialToggleCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.ChangeSessionUpliftedJourneyCollection)]
public class ChangeSessionUpliftedJourneySerialToggleCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.CancelSessionUpliftedJourneyCollection)]
public class CancelSessionUpliftedJourneySerialToggleCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.QuerySitesCollection)]
public class QuerySitesSerialToggleCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.MultiServiceJointBookingsCollection)]
public class MultiServiceJointBookingsCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.ReportsUpliftCollection)]
public class ReportsUpliftCollection : ICollectionFixture<object>
{
}
