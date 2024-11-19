using System.Collections.Generic;
using System.Linq;

namespace Nhs.Appointments.Api.Models;

public class ErrorMessageResponseItem
{
    public string Message { get; set; }
    public string Property { get; set; }

    public static IReadOnlyCollection<ErrorMessageResponseItem> None => Enumerable.Empty<ErrorMessageResponseItem>().ToList().AsReadOnly();
}