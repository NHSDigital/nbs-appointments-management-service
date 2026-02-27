using Nhs.Appointments.Core.Features;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Collections;

public static class FeatureToggleCollectionNames
{
    public const string SiteStatusCollection = $"{Flags.SiteStatus}Toggle";
    public const string CancelDayCollection = $"{Flags.CancelDay}Toggle";
    public const string ChangeSessionUpliftedJourneyCollection = $"{Flags.ChangeSessionUpliftedJourney}Toggle";
    public const string CancelSessionUpliftedJourneyCollection = $"{Flags.CancelSessionUpliftedJourney}Toggle";
    public const string QuerySitesCollection = $"{Flags.QuerySites}Toggle";
    public const string MultiServiceJointBookingsCollection = $"{Flags.MultiServiceJointBookings}Toggle";
    public const string ReportsUpliftCollection = $"{Flags.ReportsUplift}Toggle";
    public const string CancelDateRangeCollection = $"{Flags.CancelADateRange}Toggle";
    public const string CancelDateRangeWithBookingsCollection = $"{Flags.CancelADateRangeWithBookings}Toggle";
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

[CollectionDefinition(FeatureToggleCollectionNames.CancelDateRangeCollection)]
public class CancelDateRangeCollection : ICollectionFixture<object>
{
}

[CollectionDefinition(FeatureToggleCollectionNames.CancelDateRangeWithBookingsCollection)]
public class CancelDateRangeWithBookingsCollection : ICollectionFixture<object>
{
}
