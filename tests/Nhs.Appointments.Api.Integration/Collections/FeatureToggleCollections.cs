using Nhs.Appointments.Core.Features;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Collections;

public static class FeatureToggleCollectionNames
{
    public const string SiteStatusCollection = $"{Flags.SiteStatus}_Toggle";
    public const string CancelDayCollection = $"{Flags.CancelDay}_Toggle";
    public const string ChangeSessionUpliftedJourneyCollection = $"{Flags.ChangeSessionUpliftedJourney}_Toggle";
    public const string CancelSessionUpliftedJourneyCollection = $"{Flags.CancelSessionUpliftedJourney}_Toggle";
    public const string QuerySitesCollection = $"{Flags.QuerySites}_Toggle";
    public const string MultiServiceJointBookingsCollection = $"{Flags.MultiServiceJointBookings}_Toggle";
    public const string ReportsUpliftCollection = $"{Flags.ReportsUplift}_Toggle";
    
    public const string CancelADateRangeMultipleCollection = $"{Flags.CancelADateRange}|{Flags.CancelADateRangeWithBookings}_Toggle";
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
