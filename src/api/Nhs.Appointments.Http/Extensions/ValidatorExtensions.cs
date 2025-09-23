using FluentValidation;

namespace Nhs.Appointments.Http.Extensions;

public static class ValidatorExtensions
{
    public static async Task<IEnumerable<ErrorMessageResponseItem>> RunValidator<TModel>(this IValidator<TModel> validator, TModel model)
    {
        var validationResult = await validator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            return
                validationResult.Errors
                    .Select(x => new ErrorMessageResponseItem { Message = x.ErrorMessage, Property = x.PropertyName });
        }

        return [];
    }
}
