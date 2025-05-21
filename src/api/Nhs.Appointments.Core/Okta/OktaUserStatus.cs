namespace Nhs.Appointments.Core.Okta;

public enum OktaUserStatus
{
    /// <summary>
    ///     A fallback case. If this value is ever used we probably have a bug.
    /// </summary>
    Unknown,

    /// <summary>
    ///     The user has an existing account in Okta. It may be in various states of password recovery / suspension, but it
    ///     exists and requires no action from MYA.
    /// </summary>
    Active,

    /// <summary>
    ///     The user has been sent an email requesting them to complete their registration. This will go stale after 24 hours.
    /// </summary>
    Provisioned,

    /// <summary>
    ///     The user's account has been deprovisioned or deactivated by an Okta admin.
    /// </summary>
    Deactivated
}
