using Newtonsoft.Json;

namespace BookingsDataExtracts.Documents;

public class ContactItem
{
    [JsonProperty("type", Required = Required.Always)]
    public ContactItemType Type { get; set; }

    [JsonProperty("value", Required = Required.Always)]
    public string Value { get; set; }
}
