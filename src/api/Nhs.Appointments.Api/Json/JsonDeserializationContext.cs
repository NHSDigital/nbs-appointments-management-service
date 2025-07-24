using Nhs.Appointments.Api.Models;
using System.Collections.Generic;
using System.Threading;

namespace Nhs.Appointments.Api.Json;
public static class JsonDeserializationContext
{
    private static AsyncLocal<List<ErrorMessageResponseItem>> _errors = new();

    public static void AddError(string property, string message)
    {
        if (_errors.Value == null)
            _errors.Value = new List<ErrorMessageResponseItem>();

        _errors.Value.Add(new ErrorMessageResponseItem
        {
            Property = property,
            Message = message
        });
    }

    public static IReadOnlyCollection<ErrorMessageResponseItem> GetErrors()
        => _errors.Value ?? new List<ErrorMessageResponseItem>();

    public static void Clear() => _errors.Value = null;
}
