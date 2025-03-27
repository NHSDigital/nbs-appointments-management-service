namespace Nhs.Appointments.Core.Features;

public static class Flags
{
    public const string OktaEnabled = "OktaEnabled";
    public const string JointBookings = "JointBookings";

    #region TestFlags
    //a simple on/off global flag
    public const string TestFeatureEnabled = "TestFeatureEnabled";
    //a flag configured to be enabled for a pre-defined time period
    public const string TestFeatureTimeWindowEnabled = "TestFeatureTimeWindowEnabled";
    //a flag configured to be enabled for a pre-defined time period
    public const string TestFeatureTimeWindowDisabled = "TestFeatureTimeWindowDisabled";
    //a flag configured to return enabled/disabled randomly on each request at the provided probability percentage
    public const string TestFeaturePercentageEnabled = "TestFeaturePercentageEnabled";
    //a flag configured to be enabled for a specific cohort of users
    public const string TestFeatureUsersEnabled = "TestFeatureUsersEnabled";
    //a flag configured to be enabled for a specific collection of sites
    public const string TestFeatureSitesEnabled = "TestFeatureSitesEnabled";
    //a flag configured to be enabled for a specific collection of sites OR users. this demonstrates that targeting is LOGICAL OR
    public const string TestFeatureSiteOrUserEnabled = "TestFeatureSiteOrUserEnabled";
    #endregion
}
