using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nhs.Appointments.Api.Models;

public class TypeFixDocumentFilter(IEnumerable<Type> typesToFix, TimeProvider time) : IDocumentFilter
{    
    public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
    {
        foreach (var schema in document.Components.Schemas)
        {
            var type = typesToFix.SingleOrDefault(t => t.Name.ToLower() == schema.Key.ToLower());
            if (type == null)
                continue;

            if (schema.Value.Type == "object")
            {
                var updatedSchema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>()
                };

                var clrProperties = type.GetProperties();

                foreach (var prop in schema.Value.Properties)
                {
                    var targetProperty = clrProperties.Single(p => p.Name.ToLower() == prop.Key.ToLower());
                    updatedSchema.Properties.Add(prop.Key, FixupSchema(targetProperty.PropertyType, prop.Value));                    
                }

                document.Components.Schemas[schema.Key] = updatedSchema;
            }
        }
    }

    private OpenApiSchema FixupSchema(Type type, OpenApiSchema originalSchema) 
    {        
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if(underlyingType.IsEnum)
        {
            return new OpenApiSchema
            {
                Type = "string",
                Enum = type.GetEnumNames().Select(n => new OpenApiString(n)).Cast<IOpenApiAny>().ToList(),
                Default = new OpenApiString(type.GetEnumNames().Last())
            };
        }
        else if(underlyingType == typeof(DateOnly))
        {
            return new OpenApiSchema
            {
                Type = "string",
                Format = "date",
                Default = new OpenApiString(time.GetLocalNow().ToString("yyyy-MM-dd")),
                Description = "Date format must be yyyy-MM-dd"
            };
        }
        else if(underlyingType == typeof(TimeOnly))
        {
            return new OpenApiSchema
            {
                Type = "string",
                Format = "time",
                Default = new OpenApiString("09:00"),
                Description = "24 hour time format e.g. HH:mm"
            };
        }
        else if(underlyingType == typeof(DateTime))
        {
            return new OpenApiSchema
            {
                Type = "string",
                Format = "date-time",
                Default = new OpenApiString(time.GetLocalNow().ToString("yyyy-MM-dd HH:mm")),
                Description = "Date time format must be yyyy-MM-dd HH:mm"
            };
        }
        else if(underlyingType == typeof(DayOfWeek[])) // This is quite specialized and should be made more generic in future
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema
                {
                    Type = "string",
                    Enum = typeof(DayOfWeek).GetEnumNames().Select(n => new OpenApiString(n)).Cast<IOpenApiAny>().ToList(),
                    Default = new OpenApiString("Monday")
                }
            };
        }

        return originalSchema;
    }
}

