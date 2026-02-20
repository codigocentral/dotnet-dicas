using FluentValidation;
using MediatR;
using System.Linq.Expressions;

namespace Dica80.CleanArchitecture.Application.Common;

/// <summary>
/// Base validator for commands and queries
/// </summary>
/// <typeparam name="T">Type to validate</typeparam>
public abstract class BaseValidator<T> : AbstractValidator<T>
{
    protected void ValidateRequiredString(string propertyName, Expression<Func<T, string?>> propertySelector)
    {
        RuleFor(propertySelector)
            .NotEmpty()
            .WithMessage($"{propertyName} is required")
            .MaximumLength(255)
            .WithMessage($"{propertyName} must not exceed 255 characters");
    }

    protected void ValidateRequiredEmail(Expression<Func<T, string?>> propertySelector)
    {
        RuleFor(propertySelector)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");
    }

    protected void ValidateRequiredId(string propertyName, Expression<Func<T, Guid>> propertySelector)
    {
        RuleFor(propertySelector)
            .NotEmpty()
            .WithMessage($"{propertyName} is required");
    }

    protected void ValidateOptionalString(string propertyName, Expression<Func<T, string?>> propertySelector, int maxLength = 255)
    {
        RuleFor(propertySelector)
            .MaximumLength(maxLength)
            .WithMessage($"{propertyName} must not exceed {maxLength} characters")
            .When(x => !string.IsNullOrEmpty(propertySelector.Compile()(x)));
    }

    protected void ValidateDescription(Expression<Func<T, string?>> propertySelector)
    {
        RuleFor(propertySelector)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(propertySelector.Compile()(x)));
    }

    protected void ValidateEnum<TEnum>(string propertyName, Expression<Func<T, TEnum>> propertySelector) where TEnum : struct, Enum
    {
        RuleFor(propertySelector)
            .IsInEnum()
            .WithMessage($"{propertyName} must be a valid value");
    }

    protected void ValidatePositiveDecimal(string propertyName, Expression<Func<T, decimal>> propertySelector)
    {
        RuleFor(propertySelector)
            .GreaterThan(0)
            .WithMessage($"{propertyName} must be greater than 0");
    }

    protected void ValidateNonNegativeDecimal(string propertyName, Expression<Func<T, decimal>> propertySelector)
    {
        RuleFor(propertySelector)
            .GreaterThanOrEqualTo(0)
            .WithMessage($"{propertyName} must be greater than or equal to 0");
    }

    protected void ValidateDateRange(string propertyName, Expression<Func<T, DateTime?>> propertySelector)
    {
        RuleFor(propertySelector)
            .GreaterThan(DateTime.UtcNow.AddDays(-1))
            .WithMessage($"{propertyName} cannot be in the past")
            .When(x => propertySelector.Compile()(x).HasValue);
    }
}

/// <summary>
/// Validation behavior for MediatR pipeline
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(failures);
        }

        return await next();
    }
}
