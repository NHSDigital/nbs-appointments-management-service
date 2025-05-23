﻿using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class UserIdentityStatus
{
    [JsonProperty("extantInSite")] public required bool ExtantInSite { get; set; }

    [JsonProperty("extantInIdentityProvider")]
    public required bool ExtantInIdentityProvider { get; set; }

    [JsonProperty("identityProvider")] public required IdentityProvider IdentityProvider { get; set; }

    [JsonProperty("meetsWhitelistRequirements")]
    public required bool MeetsWhitelistRequirements { get; set; }
}

public enum IdentityProvider
{
    Unknown,
    NhsMail,
    Okta
}
