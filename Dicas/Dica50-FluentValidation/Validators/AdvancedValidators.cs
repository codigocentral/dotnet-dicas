using FluentValidation;
using Dica50.FluentValidation.Models;
using Dica50.FluentValidation.Services;

namespace Dica50.FluentValidation.Validators;

// Validator com validações assíncronas
public class CompanyValidator : AbstractValidator<Company>
{
    private readonly ICompanyValidationService _companyService;

    public CompanyValidator(ICompanyValidationService companyService)
    {
        _companyService = companyService;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da empresa é obrigatório")
            .Length(2, 200).WithMessage("Nome deve ter entre 2 e 200 caracteres")
            .MustAsync(BeUniqueCompanyName).WithMessage("Nome da empresa já existe");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("CNPJ é obrigatório")
            .Must(BeValidCnpj).WithMessage("CNPJ deve ter formato válido")
            .MustAsync(BeUniqueTaxId).WithMessage("CNPJ já está cadastrado");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter formato válido")
            .MustAsync(BeUniqueEmail).WithMessage("Email já está em uso");

        RuleFor(x => x.Industry)
            .NotEmpty().WithMessage("Setor é obrigatório")
            .MaximumLength(100).WithMessage("Setor não pode exceder 100 caracteres");

        RuleFor(x => x.EmployeeCount)
            .GreaterThan(0).WithMessage("Número de funcionários deve ser maior que zero");

        RuleFor(x => x.AnnualRevenue)
            .GreaterThanOrEqualTo(0).WithMessage("Receita anual não pode ser negativa");

        RuleFor(x => x.FoundedDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Data de fundação não pode ser no futuro")
            .GreaterThan(new DateTime(1800, 1, 1)).WithMessage("Data de fundação muito antiga");

        RuleFor(x => x.Website)
            .Must(BeAValidUrl).WithMessage("Website deve ter URL válida")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.HeadquartersAddress)
            .NotNull().WithMessage("Endereço da sede é obrigatório")
            .SetValidator(new AddressValidator());

        // Validação de consistência entre tamanho da empresa e número de funcionários
        RuleFor(x => x.EmployeeCount)
            .Must((company, employeeCount) => ValidateCompanySizeConsistency(company.Size, employeeCount))
            .WithMessage("Número de funcionários não é consistente com o tamanho da empresa");

        // Validação de certificações
        RuleFor(x => x.Certifications)
            .Must(certifications => certifications.All(c => !string.IsNullOrWhiteSpace(c)))
            .WithMessage("Certificações não podem estar vazias");
    }

    private async Task<bool> BeUniqueCompanyName(string name, CancellationToken cancellationToken)
    {
        return await _companyService.IsCompanyNameUniqueAsync(name, cancellationToken);
    }

    private async Task<bool> BeUniqueTaxId(string taxId, CancellationToken cancellationToken)
    {
        return await _companyService.IsTaxIdUniqueAsync(taxId, cancellationToken);
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return await _companyService.IsEmailUniqueAsync(email, cancellationToken);
    }

    private static bool BeValidCnpj(string cnpj)
    {
        // Simplificada - em produção usaria validação completa de CNPJ
        return cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Length == 14;
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private static bool ValidateCompanySizeConsistency(CompanySize size, int employeeCount)
    {
        return size switch
        {
            CompanySize.Startup => employeeCount <= 10,
            CompanySize.Small => employeeCount is > 10 and <= 50,
            CompanySize.Medium => employeeCount is > 50 and <= 250,
            CompanySize.Large => employeeCount is > 250 and <= 1000,
            CompanySize.Enterprise => employeeCount > 1000,
            _ => false
        };
    }
}

// Validator complexo para Order com múltiplas regras de negócio
public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Número do pedido é obrigatório")
            .Matches(@"^ORD-\d{6}$").WithMessage("Número do pedido deve ter formato ORD-123456");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("ID do cliente deve ser maior que zero");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Pedido deve ter pelo menos um item")
            .Must(items => items.Count <= 50).WithMessage("Máximo de 50 itens por pedido");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Custo de envio não pode ser negativo");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Desconto não pode ser negativo")
            .Must((order, discount) => discount <= CalculateSubtotal(order.Items))
            .WithMessage("Desconto não pode ser maior que o subtotal");

        RuleFor(x => x.CouponCode)
            .Matches(@"^[A-Z0-9]{6,12}$").WithMessage("Código do cupom deve ter entre 6-12 caracteres alfanuméricos")
            .When(x => !string.IsNullOrEmpty(x.CouponCode));

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Método de pagamento inválido");

        // Validação condicional para referência de pagamento
        RuleFor(x => x.PaymentReference)
            .NotEmpty().WithMessage("Referência de pagamento é obrigatória")
            .When(x => x.PaymentMethod != PaymentMethod.Cash);

        RuleFor(x => x.OrderDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Data do pedido não pode ser no futuro");

        // Validações de datas sequenciais
        RuleFor(x => x.ShippingDate)
            .GreaterThanOrEqualTo(x => x.OrderDate).WithMessage("Data de envio deve ser posterior à data do pedido")
            .When(x => x.ShippingDate.HasValue);

        RuleFor(x => x.DeliveryDate)
            .GreaterThanOrEqualTo(x => x.ShippingDate).WithMessage("Data de entrega deve ser posterior à data de envio")
            .When(x => x.DeliveryDate.HasValue && x.ShippingDate.HasValue);

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Endereço de entrega é obrigatório")
            .SetValidator(new AddressValidator());

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(500).WithMessage("Instruções especiais não podem exceder 500 caracteres");

        // Validação de status
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status do pedido inválido");

        // Validações de consistência de status e datas
        RuleFor(x => x)
            .Must(ValidateStatusConsistency)
            .WithMessage("Status do pedido inconsistente com as datas");

        // Validação do valor total
        RuleFor(x => x)
            .Must(order => CalculateTotal(order) > 0)
            .WithMessage("Valor total do pedido deve ser maior que zero");
    }

    private static decimal CalculateSubtotal(List<OrderItem> items)
    {
        return items.Sum(item => (item.UnitPrice * item.Quantity) - item.Discount);
    }

    private static decimal CalculateTotal(Order order)
    {
        var subtotal = CalculateSubtotal(order.Items);
        return subtotal + order.ShippingCost - order.Discount;
    }

    private static bool ValidateStatusConsistency(Order order)
    {
        return order.Status switch
        {
            OrderStatus.Pending => !order.ShippingDate.HasValue && !order.DeliveryDate.HasValue,
            OrderStatus.Confirmed => !order.ShippingDate.HasValue && !order.DeliveryDate.HasValue,
            OrderStatus.Processing => !order.ShippingDate.HasValue && !order.DeliveryDate.HasValue,
            OrderStatus.Shipped => order.ShippingDate.HasValue && !order.DeliveryDate.HasValue,
            OrderStatus.Delivered => order.ShippingDate.HasValue && order.DeliveryDate.HasValue,
            OrderStatus.Cancelled => true, // Cancelado pode ter qualquer estado
            OrderStatus.Refunded => true, // Reembolsado pode ter qualquer estado
            _ => false
        };
    }
}

// Validator para OrderItem
public class OrderItemValidator : AbstractValidator<OrderItem>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ID do produto deve ser maior que zero");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Nome do produto é obrigatório")
            .MaximumLength(200).WithMessage("Nome do produto não pode exceder 200 caracteres");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero")
            .LessThanOrEqualTo(100).WithMessage("Máximo de 100 unidades por item");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Preço unitário deve ser maior que zero");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Desconto não pode ser negativo")
            .LessThanOrEqualTo(x => x.UnitPrice * x.Quantity)
            .WithMessage("Desconto não pode ser maior que o valor do item");
    }
}

// Validator customizado com regras de negócio específicas
public class BusinessRuleValidator : AbstractValidator<User>
{
    public BusinessRuleValidator()
    {
        // Regra: Usuários Premium devem ter mais de 18 anos
        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Usuários Premium devem ter pelo menos 18 anos")
            .When(x => x.UserType == UserType.Premium);

        // Regra: Administradores devem ter email corporativo
        RuleFor(x => x.Email)
            .Must(email => email.EndsWith("@company.com") || email.EndsWith("@admin.com"))
            .WithMessage("Administradores devem usar email corporativo")
            .When(x => x.UserType == UserType.Admin);

        // Regra: Usuários com salário alto devem ser Premium ou Admin
        RuleFor(x => x.UserType)
            .Must(userType => userType == UserType.Premium || userType == UserType.Admin)
            .WithMessage("Usuários com salário acima de R$ 10.000 devem ser Premium ou Admin")
            .When(x => x.Salary.HasValue && x.Salary.Value > 10000);

        // Regra: Endereço obrigatório para usuários Premium
        RuleFor(x => x.Address)
            .NotNull()
            .WithMessage("Endereço é obrigatório para usuários Premium")
            .When(x => x.UserType == UserType.Premium);

        // Regra: Telefone obrigatório para Administradores
        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Telefone é obrigatório para Administradores")
            .When(x => x.UserType == UserType.Admin);
    }
}

// Validator para cenários de cross-field validation
public class CrossFieldValidator : AbstractValidator<User>
{
    public CrossFieldValidator()
    {
        // Validação que considera múltiplos campos
        RuleFor(x => x)
            .Must(user => user.Age >= 18 || user.UserType == UserType.Guest)
            .WithMessage("Menores de 18 anos só podem ser usuários Guest")
            .WithName("UserAgeRestriction");

        RuleFor(x => x)
            .Must(user => !user.IsActive || user.UserType != UserType.Guest)
            .WithMessage("Usuários Guest não podem estar ativos")
            .WithName("GuestActiveRestriction");

        RuleFor(x => x)
            .Must(user => user.CreatedAt <= DateTime.Now)
            .WithMessage("Data de criação não pode ser no futuro")
            .WithName("CreationDateValidation");

        // Validação complexa entre idade e data de nascimento
        RuleFor(x => x)
            .Must(user => Math.Abs(CalculateAge(user.BirthDate) - user.Age) <= 1)
            .WithMessage("Idade deve ser consistente com a data de nascimento")
            .WithName("AgeConsistencyValidation");
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}
