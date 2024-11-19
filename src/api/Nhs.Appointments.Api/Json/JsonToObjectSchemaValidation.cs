using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Nhs.Appointments.Api.Json
{
    public static class JsonToObjectSchemaValidation
    {
        public static List<ErrorMessageResponseItem> ValidateConversion<TRequest>(string json)
        {
            JsonDocument document;
            var errorList = new List<ErrorMessageResponseItem>();
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch(System.Text.Json.JsonException)
            {
                errorList.Add(new ErrorMessageResponseItem { Property = "document", Message = "The json is not properly formatted" });
                return errorList;
            }
            
            var requestType = typeof(TRequest);

            if (requestType.IsArray)
            {
                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    errorList.Add(new ErrorMessageResponseItem { Property = "", Message = "Expected an array but got " + document.RootElement.ValueKind.ToString() });
                } 
                else
                {
                    errorList = CheckArray(document.RootElement, requestType.GetElementType(), "root");
                }
            }
            else
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    errorList.Add(new ErrorMessageResponseItem { Property = "", Message = "Expected an object but got " + document.RootElement.ValueKind.ToString() });
                }
                else
                {                    
                    errorList = CheckObject<TRequest>(document.RootElement);
                }
            }

            return errorList;
        }

        private static List<ErrorMessageResponseItem> CheckArray(JsonElement json, Type arrayType, string path)
        {
            var errors = new List<ErrorMessageResponseItem>();
            var enumerator = json.EnumerateArray();
            int elementIndex = 0;
            while(enumerator.MoveNext())
            {
                var typeErrors = CheckType(arrayType, enumerator.Current, $"{path}[{elementIndex}]");
                errors.AddRange(typeErrors);
                elementIndex++;
            }
            return errors;
        }

        private static List<ErrorMessageResponseItem> CheckObject<TObj>(JsonElement json)
        {
            var objType = typeof(TObj);
            return CheckObject(json, objType);
        }

        private static List<ErrorMessageResponseItem> CheckObject(JsonElement json, Type objType)
        {            
            var objectEnumerator = json.EnumerateObject();
            var errors = new List<ErrorMessageResponseItem>();

            // If property type is object then we are expecting dynamic data we cannot validate further
            if (objType == typeof(Object)) 
                return errors;

            while (objectEnumerator.MoveNext())
            {
                var jsonProperty = objectEnumerator.Current;
                var objProperty = GetProperty(jsonProperty.Name, objType);

                if (objProperty == null)
                {
                    errors.Add(new ErrorMessageResponseItem { Property = jsonProperty.Name, Message = "The property does not exist on the request type" });
                }
                else
                {                    
                    var propErrors = CheckType(objProperty.PropertyType, jsonProperty);
                    errors.AddRange(propErrors);                    
                }
            }

            return errors;
        }

        private static List<ErrorMessageResponseItem> CheckType(Type type, JsonProperty json)
        {
            return CheckType(type, json.Value, json.Name);
        }

        private static List<ErrorMessageResponseItem> CheckType(Type type, JsonElement json, string path)
        {
            var errorMessage = "";
            var errors = new List<ErrorMessageResponseItem>();

            switch (type.Name)
            {
                case nameof(Int32):
                    if (json.ValueKind != JsonValueKind.Number)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = "Expected a number but found " + json.ValueKind });
                    else if (json.TryGetInt32(out var val) == false)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = "Expected an integer but found a floating point number" });
                    break;
                case nameof(TimeOnly):
                    errorMessage = $"Times should be provided as a string in the following format {DateTimeFormats.TimeOnly}";
                    if (json.ValueKind != JsonValueKind.String)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = errorMessage });
                    else if (TimeOnly.TryParseExact(json.GetString(), DateTimeFormats.TimeOnly, null, System.Globalization.DateTimeStyles.None, out var _) == false)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = errorMessage });
                    break;
                case nameof(DateOnly):
                    errorMessage = $"Dates should be provided as a string in the following format {DateTimeFormats.DateOnly}";
                    if (json.ValueKind != JsonValueKind.String)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = errorMessage });
                    else if (DateOnly.TryParseExact(json.GetString(), DateTimeFormats.DateOnly, null, System.Globalization.DateTimeStyles.None, out var _) == false)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = errorMessage });
                    break;
                case nameof(DateTime):
                    errorMessage = $"Date time data should be provided as a string in the following format {DateTimeFormats.DateTime}";
                    if (json.ValueKind != JsonValueKind.String)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = errorMessage });
                    else if (DateTime.TryParseExact(json.GetString(), DateTimeFormats.DateTime, null, System.Globalization.DateTimeStyles.None, out var _) == false)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = errorMessage });
                    break;
                case nameof(Boolean):
                    if (json.ValueKind != JsonValueKind.True && json.ValueKind != JsonValueKind.False)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = "Expected a boolean value but found " + json.ValueKind });
                    break;
                case nameof(String):
                    if (json.ValueKind != JsonValueKind.String)
                        errors.Add(new ErrorMessageResponseItem { Property = path, Message = "Expected a string but found " + json.ValueKind });
                    break;
            }

            if (type.IsArray)
            {
                if (json.ValueKind != JsonValueKind.Array)
                    errors.Add(new ErrorMessageResponseItem { Property = path, Message = "Expected an array but found " + json.ValueKind });
                else
                {
                    var nestedErrors = CheckArray(json, type.GetElementType(), path);
                    errors.AddRange(nestedErrors);
                }                
            }

            if (type.IsEnum)
            {
                if (json.ValueKind != JsonValueKind.String)
                    errors.Add(new ErrorMessageResponseItem { Property = path, Message = "Expected a string value but found " + json.ValueKind });
                else if (Enum.TryParse(type, json.GetString(), out var val) == false)
                    errors.Add(new ErrorMessageResponseItem { Property = path, Message = $"{json.GetString()} is not a valid value" });
            }

            var classesToIgnore = new[] { nameof(String) };

            if (type.IsClass && !type.IsArray && classesToIgnore.Contains(type.Name) == false)
            {
                if(json.ValueKind != JsonValueKind.Object)
                    errors.Add(new ErrorMessageResponseItem { Property = path, Message = "Expected an object but found " + json.ValueKind });
                else
                {
                    var nestedErrors = CheckObject(json, type);
                    errors.AddRange(nestedErrors);
                }
            }

            return errors;
        }

        private static PropertyInfo GetProperty(string name, Type type)
        {
            // Look for JSON attributes first
            var matchByJsonProp = type.GetProperties().Where(p => p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == name).SingleOrDefault();
            return matchByJsonProp ?? type.GetProperties().Where(p => p.Name.ToLower() == name.ToLower()).SingleOrDefault();
        }
    }
}
