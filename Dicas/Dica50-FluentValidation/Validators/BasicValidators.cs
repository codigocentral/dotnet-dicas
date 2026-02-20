using FluentValidation;
using Dica50.FluentValidation.Models;

namespace Dica50.FluentValidation.Validators;

// Validator básico para User
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .Length(2, 100).WithMessage("Nome deve ter entre 2 e 100 caracteres")
            .Matches("^[a-zA-ZÀ-ÿ\\s]+$").WithMessage("Nome deve conter apenas letras e espaços");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(255).WithMessage("Email não pode exceder 255 caracteres");

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 120).WithMessage("Idade deve estar entre 0 e 120 anos");

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Data de nascimento não pode ser no futuro")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Data de nascimento muito antiga");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?\d{10,15}$").WithMessage("Telefone deve ter formato válido")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.UserType)
            .IsInEnum().WithMessage("Tipo de usuário inválido");

        RuleFor(x => x.Interests)
            .Must(interests => interests.Count <= 10)
            .WithMessage("Máximo de 10 interesses permitidos");

        RuleFor(x => x.Website)
            .Must(BeAValidUrl).WithMessage("Website deve ter URL válida")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salário deve ser maior que zero")
            .When(x => x.Salary.HasValue);

        // Validação de objeto aninhado
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator())
            .When(x => x.Address != null);

        // Validação condicional
        RuleFor(x => x.Age)
            .Must((user, age) => CalculateAgeFromBirthDate(user.BirthDate) == age)
            .WithMessage("Idade não confere com a data de nascimento");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private static int CalculateAgeFromBirthDate(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}

// Validator para Address
public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Rua é obrigatória")
            .MaximumLength(200).WithMessage("Rua não pode exceder 200 caracteres");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Cidade é obrigatória")
            .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("Estado é obrigatório")
            .Length(2).WithMessage("Estado deve ter 2 caracteres");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("CEP é obrigatório")
            .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP deve ter formato válido (12345-678)");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("País é obrigatório")
            .MaximumLength(50).WithMessage("País não pode exceder 50 caracteres");
    }
}

// Validator para Product com validações condicionais
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do produto é obrigatório")
            .Length(3, 200).WithMessage("Nome deve ter entre 3 e 200 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(1000).WithMessage("Descrição não pode exceder 1000 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Categoria é obrigatória")
            .MaximumLength(50).WithMessage("Categoria não pode exceder 50 caracteres");

        // Validação condicional: peso obrigatório apenas para produtos físicos
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Peso deve ser maior que zero")
            .When(x => !x.IsDigital, ApplyConditionTo.CurrentValidator)
            .WithMessage("Peso é obrigatório para produtos físicos");

        RuleFor(x => x.Weight)
            .Empty().WithMessage("Produtos digitais não devem ter peso")
            .When(x => x.IsDigital);

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 20)
            .WithMessage("Máximo de 20 tags permitidas");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.Now).WithMessage("Data de expiração deve ser no futuro")
            .When(x => x.ExpirationDate.HasValue);

        RuleFor(x => x.ManufacturerCode)
            .Matches(@"^[A-Z]{2}\d{4}$").WithMessage("Código do fabricante deve ter formato XX9999")
            .When(x => !string.IsNullOrEmpty(x.ManufacturerCode));

        RuleFor(x => x.Reviews)
            .Must(reviews => reviews.All(r => r.Rating >= 1 && r.Rating <= 5))
            .WithMessage("Todas as avaliações devem ter rating entre 1 e 5");

        // Validador para lista de reviews
        RuleForEach(x => x.Reviews)
            .SetValidator(new ProductReviewValidator());
    }
}

// Validator para ProductReview
public class ProductReviewValidator : AbstractValidator<ProductReview>
{
    public ProductReviewValidator()
    {
        RuleFor(x => x.ReviewerName)
            .NotEmpty().WithMessage("Nome do avaliador é obrigatório")
            .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres");

        RuleFor(x => x.ReviewerEmail)
            .NotEmpty().WithMessage("Email do avaliador é obrigatório")
            .EmailAddress().WithMessage("Email deve ter formato válido");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating deve estar entre 1 e 5");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comentário é obrigatório")
            .MinimumLength(10).WithMessage("Comentário deve ter pelo menos 10 caracteres")
            .MaximumLength(500).WithMessage("Comentário não pode exceder 500 caracteres");

        RuleFor(x => x.ReviewDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Data da avaliação não pode ser no futuro");
    }
}

// Validator para DTOs
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .Length(2, 100).WithMessage("Nome deve ter entre 2 e 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter formato válido");

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 120).WithMessage("Idade deve estar entre 0 e 120 anos");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?\d{10,15}$").WithMessage("Telefone deve ter formato válido")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.UserType)
            .IsInEnum().WithMessage("Tipo de usuário inválido");

        RuleFor(x => x.Interests)
            .Must(interests => interests.Count <= 10)
            .WithMessage("Máximo de 10 interesses permitidos");

        RuleFor(x => x.Address)
            .SetValidator(new CreateAddressRequestValidator())
            .When(x => x.Address != null);
    }
}

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Rua é obrigatória")
            .MaximumLength(200).WithMessage("Rua não pode exceder 200 caracteres");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Cidade é obrigatória")
            .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("Estado é obrigatório")
            .Length(2).WithMessage("Estado deve ter 2 caracteres");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("CEP é obrigatório")
            .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP deve ter formato válido");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("País é obrigatório")
            .MaximumLength(50).WithMessage("País não pode exceder 50 caracteres");
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .Length(2, 100).WithMessage("Nome deve ter entre 2 e 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email deve ter formato válido")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 120).WithMessage("Idade deve estar entre 0 e 120 anos")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.Phone)
            .Matches(@"^\+?\d{10,15}$").WithMessage("Telefone deve ter formato válido")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Interests)
            .Must(interests => interests!.Count <= 10)
            .WithMessage("Máximo de 10 interesses permitidos")
            .When(x => x.Interests != null);

        RuleFor(x => x.Address)
            .SetValidator(new UpdateAddressRequestValidator())
            .When(x => x.Address != null);
    }
}

public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressRequestValidator()
    {
        RuleFor(x => x.Street)
            .MaximumLength(200).WithMessage("Rua não pode exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Street));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .Length(2).WithMessage("Estado deve ter 2 caracteres")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.ZipCode)
            .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP deve ter formato válido")
            .When(x => !string.IsNullOrEmpty(x.ZipCode));

        RuleFor(x => x.Country)
            .MaximumLength(50).WithMessage("País não pode exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Country));
    }
}
