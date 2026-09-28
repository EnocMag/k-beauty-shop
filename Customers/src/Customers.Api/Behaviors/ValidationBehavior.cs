using FluentValidation;
using MediatR;
using Customers.Utils;

namespace Customers.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new FluentValidation.ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .Select(x => x.ErrorMessage)
            .Distinct()
            .ToList();

        if (failures.Count > 0)
        {
            throw new ProblemsException(MsgConstants.VALIDATION_ERROR, failures);
        }

        return await next();
    }
}
