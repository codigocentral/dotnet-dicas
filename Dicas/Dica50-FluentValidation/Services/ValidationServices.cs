namespace Dica50.FluentValidation.Services;

// Interface para validações assíncronas de empresa
public interface ICompanyValidationService
{
    Task<bool> IsCompanyNameUniqueAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsTaxIdUniqueAsync(string taxId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
}

// Implementação do serviço de validação (simulada)
public class CompanyValidationService : ICompanyValidationService
{
    private readonly ILogger<CompanyValidationService> _logger;
    
    // Simulação de empresas já cadastradas
    private static readonly HashSet<string> ExistingCompanyNames = new()
    {
        "Microsoft Corporation",
        "Google LLC",
        "Amazon Inc",
        "Apple Inc"
    };

    private static readonly HashSet<string> ExistingTaxIds = new()
    {
        "12345678000100",
        "98765432000111",
        "11111111000122"
    };

    private static readonly HashSet<string> ExistingEmails = new()
    {
        "contact@microsoft.com",
        "info@google.com",
        "admin@amazon.com"
    };

    public CompanyValidationService(ILogger<CompanyValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> IsCompanyNameUniqueAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validando unicidade do nome da empresa: {CompanyName}", name);
        
        // Simula consulta ao banco de dados
        await Task.Delay(100, cancellationToken);
        
        var isUnique = !ExistingCompanyNames.Contains(name);
        
        _logger.LogInformation("Nome da empresa {CompanyName} é único: {IsUnique}", name, isUnique);
        
        return isUnique;
    }

    public async Task<bool> IsTaxIdUniqueAsync(string taxId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validando unicidade do CNPJ: {TaxId}", taxId);
        
        // Simula consulta ao banco de dados
        await Task.Delay(150, cancellationToken);
        
        var cleanTaxId = taxId.Replace(".", "").Replace("/", "").Replace("-", "");
        var isUnique = !ExistingTaxIds.Contains(cleanTaxId);
        
        _logger.LogInformation("CNPJ {TaxId} é único: {IsUnique}", taxId, isUnique);
        
        return isUnique;
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validando unicidade do email: {Email}", email);
        
        // Simula consulta ao banco de dados
        await Task.Delay(80, cancellationToken);
        
        var isUnique = !ExistingEmails.Contains(email.ToLowerInvariant());
        
        _logger.LogInformation("Email {Email} é único: {IsUnique}", email, isUnique);
        
        return isUnique;
    }
}

// Serviço para demonstrar validações em contexto de aplicação
public interface IUserService
{
    Task<Models.User> CreateUserAsync(Models.CreateUserRequest request);
    Task<Models.User> UpdateUserAsync(int id, Models.UpdateUserRequest request);
    Task<Models.User?> GetUserByIdAsync(int id);
    Task<List<Models.User>> GetAllUsersAsync();
}

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private static readonly List<Models.User> Users = new();
    private static int _nextId = 1;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<Models.User> CreateUserAsync(Models.CreateUserRequest request)
    {
        _logger.LogInformation("Criando usuário: {UserName}", request.Name);

        await Task.Delay(50); // Simula operação assíncrona

        var user = new Models.User
        {
            Id = _nextId++,
            Name = request.Name,
            Email = request.Email,
            Age = request.Age,
            BirthDate = DateTime.Today.AddYears(-request.Age),
            Phone = request.Phone,
            UserType = request.UserType,
            IsActive = true,
            CreatedAt = DateTime.Now,
            Interests = request.Interests,
            Address = request.Address != null ? new Models.Address
            {
                Street = request.Address.Street,
                City = request.Address.City,
                State = request.Address.State,
                ZipCode = request.Address.ZipCode,
                Country = request.Address.Country,
                IsDefault = true
            } : null
        };

        Users.Add(user);

        _logger.LogInformation("Usuário criado com ID: {UserId}", user.Id);
        
        return user;
    }

    public async Task<Models.User> UpdateUserAsync(int id, Models.UpdateUserRequest request)
    {
        _logger.LogInformation("Atualizando usuário ID: {UserId}", id);

        await Task.Delay(50); // Simula operação assíncrona

        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            throw new ArgumentException($"Usuário com ID {id} não encontrado");
        }

        if (!string.IsNullOrEmpty(request.Name))
            user.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;

        if (request.Age.HasValue)
        {
            user.Age = request.Age.Value;
            user.BirthDate = DateTime.Today.AddYears(-request.Age.Value);
        }

        if (request.Phone != null)
            user.Phone = request.Phone;

        if (request.Interests != null)
            user.Interests = request.Interests;

        if (request.Address != null)
        {
            if (user.Address == null)
                user.Address = new Models.Address();

            if (!string.IsNullOrEmpty(request.Address.Street))
                user.Address.Street = request.Address.Street;

            if (!string.IsNullOrEmpty(request.Address.City))
                user.Address.City = request.Address.City;

            if (!string.IsNullOrEmpty(request.Address.State))
                user.Address.State = request.Address.State;

            if (!string.IsNullOrEmpty(request.Address.ZipCode))
                user.Address.ZipCode = request.Address.ZipCode;

            if (!string.IsNullOrEmpty(request.Address.Country))
                user.Address.Country = request.Address.Country;
        }

        _logger.LogInformation("Usuário ID {UserId} atualizado com sucesso", id);
        
        return user;
    }

    public async Task<Models.User?> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("Buscando usuário ID: {UserId}", id);

        await Task.Delay(25); // Simula operação assíncrona

        var user = Users.FirstOrDefault(u => u.Id == id);
        
        _logger.LogInformation("Usuário ID {UserId} {Found}", id, user != null ? "encontrado" : "não encontrado");
        
        return user;
    }

    public async Task<List<Models.User>> GetAllUsersAsync()
    {
        _logger.LogInformation("Listando todos os usuários");

        await Task.Delay(100); // Simula operação assíncrona

        _logger.LogInformation("Retornando {UserCount} usuários", Users.Count);
        
        return Users.ToList();
    }
}

// Serviço para demonstrar validações de produtos
public interface IProductService
{
    Task<Models.Product> CreateProductAsync(Models.Product product);
    Task<Models.Product> UpdateProductAsync(int id, Models.Product product);
    Task<Models.Product?> GetProductByIdAsync(int id);
    Task<bool> IsProductNameUniqueAsync(string name);
}

public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;
    private static readonly List<Models.Product> Products = new();
    private static int _nextId = 1;

    public ProductService(ILogger<ProductService> logger)
    {
        _logger = logger;
    }

    public async Task<Models.Product> CreateProductAsync(Models.Product product)
    {
        _logger.LogInformation("Criando produto: {ProductName}", product.Name);

        await Task.Delay(75); // Simula operação assíncrona

        product.Id = _nextId++;
        Products.Add(product);

        _logger.LogInformation("Produto criado com ID: {ProductId}", product.Id);
        
        return product;
    }

    public async Task<Models.Product> UpdateProductAsync(int id, Models.Product product)
    {
        _logger.LogInformation("Atualizando produto ID: {ProductId}", id);

        await Task.Delay(50); // Simula operação assíncrona

        var existingProduct = Products.FirstOrDefault(p => p.Id == id);
        if (existingProduct == null)
        {
            throw new ArgumentException($"Produto com ID {id} não encontrado");
        }

        product.Id = id;
        var index = Products.IndexOf(existingProduct);
        Products[index] = product;

        _logger.LogInformation("Produto ID {ProductId} atualizado com sucesso", id);
        
        return product;
    }

    public async Task<Models.Product?> GetProductByIdAsync(int id)
    {
        _logger.LogInformation("Buscando produto ID: {ProductId}", id);

        await Task.Delay(25); // Simula operação assíncrona

        var product = Products.FirstOrDefault(p => p.Id == id);
        
        _logger.LogInformation("Produto ID {ProductId} {Found}", id, product != null ? "encontrado" : "não encontrado");
        
        return product;
    }

    public async Task<bool> IsProductNameUniqueAsync(string name)
    {
        _logger.LogInformation("Verificando unicidade do nome do produto: {ProductName}", name);

        await Task.Delay(100); // Simula consulta ao banco

        var isUnique = !Products.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        
        _logger.LogInformation("Nome do produto {ProductName} é único: {IsUnique}", name, isUnique);
        
        return isUnique;
    }
}
