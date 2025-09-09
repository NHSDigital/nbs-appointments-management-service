﻿using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Core.Json;

namespace DataExtract;

public class CosmosJsonSerializer : CosmosSerializer
{
    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            var desrializerSettings = new JsonSerializerSettings
            {
                Converters = { new ShortTimeOnlyJsonConverter(), new ShortDateOnlyJsonConverter(), new DayOfWeekJsonConverter(), new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
            var body = new StreamReader(stream).ReadToEndAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<T>(body, desrializerSettings);
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var serializerSettings = new JsonSerializerSettings
        {
            Converters = { new ShortTimeOnlyJsonConverter(), new ShortDateOnlyJsonConverter(), new DayOfWeekJsonConverter(), new Newtonsoft.Json.Converters.StringEnumConverter() }
        };
        var json = JsonConvert.SerializeObject(input, serializerSettings);
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }
}

