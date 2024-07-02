﻿using Newtonsoft.Json;

namespace Nhs.Appointments.ApiClient.Models
{
    public class SiteConfiguration
    {
        [JsonProperty("siteId")]
        public string SiteId { get; set; }

        [JsonProperty("informationForCitizen")]
        public string InformationForCitizen { get; set; }

        [JsonProperty("referenceNumberGroup")]
        public int ReferenceNumberGroup { get; set; }

        [JsonProperty("serviceConfiguration")]
        public IEnumerable<ServiceConfiguration> ServiceConfiguration { get; set; }
    }
}
