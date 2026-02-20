using FluentValidation;
using Dica50.FluentValidation.Models;
using Dica50.FluentValidation.Validators;
using Dica50.FluentValidation.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Adicionar serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Dica 50: FluentValidation API", 
        Version = "v1",
        Description = "Demonstração completa de FluentValidation em ASP.NET Core"
    });
});

// Registrar FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

// Registrar validators específicos
builder.Services.AddScoped<IValidator<User>, UserValidator>();
builder.Services.AddScoped<IValidator<Address>, AddressValidator>();
builder.Services.AddScoped<IValidator<Product>, ProductValidator>();
builder.Services.AddScoped<IValidator<ProductReview>, ProductReviewValidator>();
builder.Services.AddScoped<IValidator<Company>, CompanyValidator>();
builder.Services.AddScoped<IValidator<Order>, OrderValidator>();
builder.Services.AddScoped<IValidator<OrderItem>, OrderItemValidator>();
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
builder.Services.AddScoped<IValidator<CreateAddressRequest>, CreateAddressRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateAddressRequest>, UpdateAddressRequestValidator>();

// Registrar validators de regras de negócio
builder.Services.AddScoped<BusinessRuleValidator>();
builder.Services.AddScoped<CrossFieldValidator>();

// Registrar serviços de aplicação
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICompanyValidationService, CompanyValidationService>();

var app = builder.Build();

// Configurar pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dica 50: FluentValidation API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.MapControllers();

// Endpoint de demonstração principal
app.MapGet("/", () => Results.Redirect("/swagger"));

// Demonstrações programáticas
app.MapGet("/demo/basic-validation", async (IValidator<User> userValidator) =>
{
    Console.WriteLine("🎯 DEMONSTRAÇÃO: Validação Básica de Usuário");
    Console.WriteLine("=" + new string('=', 50));

    var user = new User
    {
        Name = "", // Inválido: vazio
        Email = "email-invalido", // Inválido: formato
        Age = -5, // Inválido: negativo
        BirthDate = DateTime.Today.AddYears(1), // Inválido: futuro
        Phone = "123", // Inválido: muito curto
        UserType = (UserType)999 // Inválido: não existe
    };

    var result = await userValidator.ValidateAsync(user);
    
    return new
    {
        IsValid = result.IsValid,
        Errors = result.Errors.Select(e => new
        {
            Field = e.PropertyName,
            Message = e.ErrorMessage,
            AttemptedValue = e.AttemptedValue
        }).ToArray()
    };
});

app.MapGet("/demo/conditional-validation", async (IValidator<Product> productValidator) =>
{
    Console.WriteLine("🎯 DEMONSTRAÇÃO: Validação Condicional de Produto");
    Console.WriteLine("=" + new string('=', 50));

    var physicalProduct = new Product
    {
        Name = "Livro Físico",
        Description = "Um livro impresso",
        Price = 29.99m,
        Category = "Livros",
        IsDigital = false,
        Weight = null // Inválido: produto físico deve ter peso
    };

    var digitalProduct = new Product
    {
        Name = "E-book",
        Description = "Um livro digital",
        Price = 19.99m,
        Category = "Livros",
        IsDigital = true,
        Weight = 0.5m // Inválido: produto digital não deve ter peso
    };

    var physicalResult = await productValidator.ValidateAsync(physicalProduct);
    var digitalResult = await productValidator.ValidateAsync(digitalProduct);

    return new
    {
        PhysicalProduct = new
        {
            IsValid = physicalResult.IsValid,
            Errors = physicalResult.Errors.Select(e => e.ErrorMessage).ToArray()
        },
        DigitalProduct = new
        {
            IsValid = digitalResult.IsValid,
            Errors = digitalResult.Errors.Select(e => e.ErrorMessage).ToArray()
        }
    };
});

app.MapGet("/demo/async-validation", async (IValidator<Company> companyValidator) =>
{
    Console.WriteLine("🎯 DEMONSTRAÇÃO: Validação Assíncrona de Empresa");
    Console.WriteLine("=" + new string('=', 50));

    var companies = new[]
    {
        new Company
        {
            Name = "Nova Empresa", // Único
            TaxId = "12345678000199", // Válido e único
            Email = "contato@novaempresa.com", // Único
            Industry = "Tecnologia",
            EmployeeCount = 25,
            AnnualRevenue = 500000,
            FoundedDate = new DateTime(2020, 1, 1),
            HeadquartersAddress = new Address
            {
                Street = "Rua das Flores, 123",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01234-567",
                Country = "Brasil"
            },
            Size = CompanySize.Small
        },
        new Company
        {
            Name = "Microsoft Corporation", // Duplicado
            TaxId = "12345678000100", // Duplicado
            Email = "contact@microsoft.com", // Duplicado
            Industry = "Tecnologia",
            EmployeeCount = 200000,
            AnnualRevenue = 200000000,
            FoundedDate = new DateTime(1975, 4, 4),
            HeadquartersAddress = new Address
            {
                Street = "One Microsoft Way",
                City = "Redmond",
                State = "WA",
                ZipCode = "98052-0000",
                Country = "USA"
            },
            Size = CompanySize.Enterprise
        }
    };

    var results = new List<object>();

    foreach (var company in companies)
    {
        var result = await companyValidator.ValidateAsync(company);
        results.Add(new
        {
            CompanyName = company.Name,
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => new
            {
                Field = e.PropertyName,
                Message = e.ErrorMessage
            }).ToArray()
        });
    }

    return results;
});

app.MapGet("/demo/business-rules", async (BusinessRuleValidator businessValidator) =>
{
    Console.WriteLine("🎯 DEMONSTRAÇÃO: Validação de Regras de Negócio");
    Console.WriteLine("=" + new string('=', 50));

    var users = new[]
    {
        new User
        {
            Name = "João Premium",
            Email = "joao@gmail.com",
            Age = 17, // Inválido: Premium deve ter 18+
            UserType = UserType.Premium,
            Salary = 5000
        },
        new User
        {
            Name = "Admin User",
            Email = "admin@gmail.com", // Inválido: admin deve usar email corporativo
            Age = 30,
            UserType = UserType.Admin,
            Phone = null // Inválido: admin deve ter telefone
        },
        new User
        {
            Name = "Rich User",
            Email = "rich@gmail.com",
            Age = 35,
            UserType = UserType.Regular, // Inválido: salário alto deve ser Premium/Admin
            Salary = 15000
        }
    };

    var results = new List<object>();

    foreach (var user in users)
    {
        var result = await businessValidator.ValidateAsync(user);
        results.Add(new
        {
            UserName = user.Name,
            UserType = user.UserType.ToString(),
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => e.ErrorMessage).ToArray()
        });
    }

    return results;
});

app.MapGet("/demo/cross-field-validation", async (CrossFieldValidator crossFieldValidator) =>
{
    Console.WriteLine("🎯 DEMONSTRAÇÃO: Validação Cross-Field");
    Console.WriteLine("=" + new string('=', 50));

    var user = new User
    {
        Name = "Test User",
        Email = "test@example.com",
        Age = 25,
        BirthDate = DateTime.Today.AddYears(-30), // Inconsistente com idade
        UserType = UserType.Guest,
        IsActive = true, // Inválido: Guest não pode estar ativo
        CreatedAt = DateTime.Now.AddDays(1) // Inválido: criação no futuro
    };

    var result = await crossFieldValidator.ValidateAsync(user);

    return new
    {
        IsValid = result.IsValid,
        Errors = result.Errors.Select(e => new
        {
            Rule = e.PropertyName,
            Message = e.ErrorMessage
        }).ToArray()
    };
});

app.MapGet("/demo/complex-order", async (IValidator<Order> orderValidator) =>
{
    Console.WriteLine("🎯 DEMONSTRAÇÃO: Validação Complexa de Pedido");
    Console.WriteLine("=" + new string('=', 50));

    var order = new Order
    {
        OrderNumber = "ORD-123456",
        CustomerId = 1,
        Items = new List<OrderItem>
        {
            new()
            {
                ProductId = 1,
                ProductName = "Produto A",
                Quantity = 2,
                UnitPrice = 50.00m,
                Discount = 10.00m
            },
            new()
            {
                ProductId = 2,
                ProductName = "Produto B",
                Quantity = 1,
                UnitPrice = 30.00m,
                Discount = 150.00m // Inválido: desconto maior que valor
            }
        },
        ShippingCost = 15.00m,
        Discount = 200.00m, // Inválido: desconto maior que subtotal
        PaymentMethod = PaymentMethod.CreditCard,
        PaymentReference = "CC-123456789",
        Status = OrderStatus.Delivered,
        OrderDate = DateTime.Now.AddDays(-7),
        ShippingDate = null, // Inválido: entregue mas sem data de envio
        DeliveryDate = DateTime.Now.AddDays(-1),
        ShippingAddress = new Address
        {
            Street = "Rua das Entregas, 456",
            City = "São Paulo",
            State = "SP",
            ZipCode = "12345-678",
            Country = "Brasil"
        }
    };

    var result = await orderValidator.ValidateAsync(order);

    // Calcular valores para contexto
    var subtotal = order.Items.Sum(item => (item.UnitPrice * item.Quantity) - item.Discount);
    var total = Math.Max(0, subtotal + order.ShippingCost - order.Discount);

    return new
    {
        IsValid = result.IsValid,
        OrderSummary = new
        {
            ItemCount = order.Items.Count,
            Subtotal = subtotal,
            ShippingCost = order.ShippingCost,
            Discount = order.Discount,
            Total = total
        },
        Errors = result.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => string.IsNullOrEmpty(g.Key) ? "General" : g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            )
    };
});

Console.WriteLine("🚀 DICA 50: FluentValidation - Validação Robusta e Elegante");
Console.WriteLine("=" + new string('=', 70));
Console.WriteLine();
Console.WriteLine("📋 Funcionalidades Demonstradas:");
Console.WriteLine("  ✅ Validações básicas (NotEmpty, Length, Range, etc.)");
Console.WriteLine("  ✅ Validações condicionais (When)");
Console.WriteLine("  ✅ Validações assíncronas (MustAsync)");
Console.WriteLine("  ✅ Validações de objetos aninhados (SetValidator)");
Console.WriteLine("  ✅ Validações de coleções (RuleForEach)");
Console.WriteLine("  ✅ Cross-field validation");
Console.WriteLine("  ✅ Regras de negócio customizadas");
Console.WriteLine("  ✅ Integração com ASP.NET Core");
Console.WriteLine("  ✅ Mensagens de erro personalizadas");
Console.WriteLine("  ✅ Validators para DTOs e entities");
Console.WriteLine();
Console.WriteLine("🌐 Endpoints disponíveis:");
Console.WriteLine("  📄 /swagger - Documentação da API");
Console.WriteLine("  🧪 /demo/basic-validation - Validação básica");
Console.WriteLine("  🔀 /demo/conditional-validation - Validação condicional");
Console.WriteLine("  ⚡ /demo/async-validation - Validação assíncrona");
Console.WriteLine("  🏢 /demo/business-rules - Regras de negócio");
Console.WriteLine("  🔗 /demo/cross-field-validation - Cross-field validation");
Console.WriteLine("  📦 /demo/complex-order - Validação complexa");
Console.WriteLine();
Console.WriteLine("🚀 Aplicação iniciada!");

app.Run();
