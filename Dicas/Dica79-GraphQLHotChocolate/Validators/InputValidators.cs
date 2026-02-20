using FluentValidation;
using Dica79.GraphQLHotChocolate.Models;

namespace Dica79.GraphQLHotChocolate.Validators;

public class CreateUserInputValidator : AbstractValidator<CreateUserInput>
{
    public CreateUserInputValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
    }
}

public class UpdateUserInputValidator : AbstractValidator<UpdateUserInput>
{
    public UpdateUserInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid user ID is required");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format is invalid")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));
    }
}

public class CreatePostInputValidator : AbstractValidator<CreatePostInput>
{
    public CreatePostInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(5, 200).WithMessage("Title must be between 5 and 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters");

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Summary cannot exceed 500 characters");
    }
}

public class UpdatePostInputValidator : AbstractValidator<UpdatePostInput>
{
    public UpdatePostInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid post ID is required");

        RuleFor(x => x.Title)
            .Length(5, 200).WithMessage("Title must be between 5 and 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .MinimumLength(10).WithMessage("Content must be at least 10 characters")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Summary cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Summary));
    }
}

public class CreateCommentInputValidator : AbstractValidator<CreateCommentInput>
{
    public CreateCommentInputValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .Length(1, 1000).WithMessage("Comment must be between 1 and 1000 characters");

        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Valid post ID is required");

        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0).WithMessage("Valid parent comment ID is required")
            .When(x => x.ParentCommentId.HasValue);
    }
}
