using System.Net;

namespace Nhs.Appointments.Api.Functions;

public class ApiResult<TResponse>
{
    public static ApiResult<TResponse> Success(TResponse response)
    {
        return new ApiResult<TResponse> { ResponseObject = response, IsSuccess = true };
    }

    public static ApiResult<TResponse> Failed(HttpStatusCode status, string reason)
    {
        return new ApiResult<TResponse> { StatusCode = status, Reason = reason, IsSuccess = false };
    }

    public TResponse ResponseObject { get; private init; }

    public HttpStatusCode StatusCode { get; private init; }

    public string Reason { get; private init; }

    public bool IsSuccess { get; private init; }
}
