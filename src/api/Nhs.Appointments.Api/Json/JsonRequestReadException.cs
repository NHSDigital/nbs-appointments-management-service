using System;

namespace Nhs.Appointments.Api.Json;

public class JsonRequestReadException : Exception
{
    public JsonRequestReadException(string message, Exception exception) : base(message, exception)
    {
    } 
}