using System.Reflection;
using BikeService.Application.Wrappers;
using FluentValidation;
using MediatR;

namespace BikeService.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var fieldErrors = new Dictionary<string, string>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            foreach (var failure in result.Errors)
                fieldErrors[failure.PropertyName] = failure.ErrorMessage;
        }

        if (fieldErrors.Count == 0)
            return await next();

        if (TryBuildFailedResult(fieldErrors, out var failedResponse))
            return failedResponse!;

        throw new ValidationException(fieldErrors.Select(e => new FluentValidation.Results.ValidationFailure(e.Key, e.Value)));
    }

    private static bool TryBuildFailedResult(Dictionary<string, string> fieldErrors, out TResponse? response)
    {
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failMethod = responseType.GetMethod(nameof(Result<object>.FailFields), BindingFlags.Public | BindingFlags.Static);
            response = (TResponse)failMethod!.Invoke(null, [fieldErrors, null])!;
            return true;
        }

        response = default;
        return false;
    }
}
