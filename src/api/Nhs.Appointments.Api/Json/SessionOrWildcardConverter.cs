using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using System;

namespace Nhs.Appointments.Api.Json;
public class SessionOrWildcardConverter : JsonConverter<SessionOrWildcard>
{
    public override SessionOrWildcard ReadJson(JsonReader reader, Type objectType, SessionOrWildcard existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var value = reader.Value?.ToString();
            return value == "*"
                ? new SessionOrWildcard {  IsWildcard = true }
                : throw new JsonSerializationException("Only '*' is allowed as a string value.");
        }
        else if (reader.TokenType == JsonToken.StartObject)
        {
            var session = serializer.Deserialize<Session>(reader);
            return new SessionOrWildcard { Session = session };
        }

        throw new JsonSerializationException("Expected '*' or a session object.");
    }

    public override void WriteJson(JsonWriter writer, SessionOrWildcard value, JsonSerializer serializer)
    {
        var wrapper = (SessionOrWildcard)value;
        if (wrapper.IsWildcard)
        {
            writer.WriteValue("*");
        }
        else if (wrapper.Session is not null)
        {
            serializer.Serialize(writer, wrapper.Session);
        }
        else
        {
            writer.WriteNull();
        }
    }
}
