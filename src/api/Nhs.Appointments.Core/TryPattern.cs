namespace Nhs.Appointments.Core;

public static class TryPattern
{
    public static TryResult<TResult> Try<TResult>(Func<TResult> action)
    {
        try
        {
            return new(true, action());
        }
        catch (Exception)
        {
            return new(false, default);
        }
    }

    public static async Task<TryResult<TResult>> TryAsync<TResult>(Func<Task<TResult>> action)
    {
        try
        {
            return new(true, await action());
        }
        catch (Exception)
        {
            return new(false, default);
        }
    }

    public record TryResult<TResult>(bool Completed, TResult Result);
}    
