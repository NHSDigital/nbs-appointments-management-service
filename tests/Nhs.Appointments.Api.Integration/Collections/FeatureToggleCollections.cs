using Nhs.Appointments.Core.Features;
using Xunit;

namespace Nhs.Appointments.Api.Integration.Collections;

public static class FeatureToggleCollectionNames
{
    public const string CancelDayCollection = $"{Flags.CancelDay}_Toggle";
    public const string MultiServiceJointBookingsCollection = $"{Flags.MultiServiceJointBookings}_Toggle";
    public const string ReportsUpliftCollection = $"{Flags.ReportsUplift}_Toggle";

    public const string CancelDateRangeAndBookingsCollection = $"{Flags.CancelADateRange}|{Flags.CancelADateRangeWithBookings}_Toggle";

    public const string TestMultipleCollection = $"{Flags.TestFeaturePercentageEnabled}|{Flags.TestFeatureSitesEnabled}|{Flags.TestFeatureUsersEnabled}_Toggle";
}

[CollectionDefinition(FeatureToggleCollectionNames.CancelDayCollection)]
public class CancelDaySerialToggleCollection : ICollectionFixture<object>
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

[CollectionDefinition(FeatureToggleCollectionNames.CancelDateRangeAndBookingsCollection)]
public class CancelDateRangeAndBookingsCollection : ICollectionFixture<object>
{
}
