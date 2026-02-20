using Microsoft.Extensions.Logging;
using Dica61.DependencyInjection.Interfaces;

namespace Dica61.DependencyInjection.Services;

// Serviço de domínio que utiliza múltiplas dependências
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IAuditLogger _auditLogger;
    private readonly INotificationService _notificationService;
    private readonly IBusinessRuleValidator _validator;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ICacheService cacheService,
        IAuditLogger auditLogger,
        INotificationService notificationService,
        IBusinessRuleValidator validator,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _auditLogger = auditLogger;
        _notificationService = notificationService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        _logger.LogInformation("Buscando usuário: {UserId}", id);

        // Verificar cache primeiro
        var cacheKey = $"user:{id}";
        var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
        if (cachedUser != null)
        {
            _logger.LogDebug("Usuário encontrado no cache: {UserId}", id);
            return cachedUser;
        }

        // Buscar no repositório
        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            // Adicionar ao cache
            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(15));
            
            // Log de auditoria
            await _auditLogger.LogActionAsync("USER_RETRIEVED", $"User {id} retrieved", id);
        }

        return user;
    }

    public async Task<User> CreateUserAsync(string name, string email)
    {
        _logger.LogInformation("Criando usuário: {Name}, {Email}", name, email);

        // Validar regras de negócio
        var validationResult = await _validator.ValidateUserCreationAsync(name, email);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors);
            _logger.LogWarning("Falha na validação para criação do usuário: {Errors}", errorMessage);
            throw new ArgumentException($"Dados inválidos: {errorMessage}");
        }

        // Criar usuário
        var user = new User
        {
            Name = name,
            Email = email,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user);

        // Log de auditoria
        await _auditLogger.LogActionAsync("USER_CREATED", $"User {createdUser.Id} created", createdUser.Id);

        // Enviar notificação de boas-vindas
        await _notificationService.SendEmailAsync(
            createdUser.Email,
            "Bem-vindo!",
            $"Olá {createdUser.Name}, sua conta foi criada com sucesso!");

        _logger.LogInformation("Usuário criado com sucesso: {UserId}", createdUser.Id);
        return createdUser;
    }

    public async Task<User> UpdateUserAsync(int id, string name, string email)
    {
        _logger.LogInformation("Atualizando usuário: {UserId}", id);

        // Validar regras de negócio
        var validationResult = await _validator.ValidateUserUpdateAsync(id, name, email);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors);
            _logger.LogWarning("Falha na validação para atualização do usuário: {Errors}", errorMessage);
            throw new ArgumentException($"Dados inválidos: {errorMessage}");
        }

        // Buscar usuário existente
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            throw new ArgumentException($"Usuário {id} não encontrado");
        }

        // Atualizar dados
        existingUser.Name = name;
        existingUser.Email = email;

        var updatedUser = await _userRepository.UpdateAsync(existingUser);

        // Invalidar cache
        await _cacheService.RemoveAsync($"user:{id}");

        // Log de auditoria
        await _auditLogger.LogActionAsync("USER_UPDATED", $"User {id} updated", id);

        _logger.LogInformation("Usuário atualizado com sucesso: {UserId}", id);
        return updatedUser;
    }

    public async Task DeleteUserAsync(int id)
    {
        _logger.LogInformation("Removendo usuário: {UserId}", id);

        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            throw new ArgumentException($"Usuário {id} não encontrado");
        }

        await _userRepository.DeleteAsync(id);

        // Invalidar cache
        await _cacheService.RemoveAsync($"user:{id}");

        // Log de auditoria
        await _auditLogger.LogActionAsync("USER_DELETED", $"User {id} deleted", id);

        _logger.LogInformation("Usuário removido com sucesso: {UserId}", id);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        _logger.LogInformation("Buscando usuários ativos");

        var cacheKey = "users:active";
        var cachedUsers = await _cacheService.GetAsync<List<User>>(cacheKey);
        if (cachedUsers != null)
        {
            _logger.LogDebug("Usuários ativos encontrados no cache");
            return cachedUsers;
        }

        var activeUsers = await _userRepository.GetActiveUsersAsync();
        var userList = activeUsers.ToList();

        // Adicionar ao cache
        await _cacheService.SetAsync(cacheKey, userList, TimeSpan.FromMinutes(5));

        // Log de auditoria
        await _auditLogger.LogActionAsync("ACTIVE_USERS_RETRIEVED", $"{userList.Count} active users retrieved");

        return userList;
    }
}

// Validador de regras de negócio
public class BusinessRuleValidator : IBusinessRuleValidator
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BusinessRuleValidator> _logger;

    public BusinessRuleValidator(IUserRepository userRepository, ILogger<BusinessRuleValidator> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateUserCreationAsync(string name, string email)
    {
        var result = new ValidationResult { IsValid = true };

        // Validar nome
        if (string.IsNullOrWhiteSpace(name))
        {
            result.Errors.Add("Nome é obrigatório");
        }
        else if (name.Length < 2)
        {
            result.Errors.Add("Nome deve ter pelo menos 2 caracteres");
        }

        // Validar email
        if (string.IsNullOrWhiteSpace(email))
        {
            result.Errors.Add("Email é obrigatório");
        }
        else if (!IsValidEmail(email))
        {
            result.Errors.Add("Email deve ter um formato válido");
        }
        else
        {
            // Verificar se email já existe
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                result.Errors.Add("Email já está em uso");
            }
        }

        result.IsValid = !result.Errors.Any();
        
        if (!result.IsValid)
        {
            _logger.LogWarning("Validação de criação de usuário falhou: {Errors}", string.Join(", ", result.Errors));
        }

        return result;
    }

    public async Task<ValidationResult> ValidateUserUpdateAsync(int id, string name, string email)
    {
        var result = new ValidationResult { IsValid = true };

        // Validar nome
        if (string.IsNullOrWhiteSpace(name))
        {
            result.Errors.Add("Nome é obrigatório");
        }
        else if (name.Length < 2)
        {
            result.Errors.Add("Nome deve ter pelo menos 2 caracteres");
        }

        // Validar email
        if (string.IsNullOrWhiteSpace(email))
        {
            result.Errors.Add("Email é obrigatório");
        }
        else if (!IsValidEmail(email))
        {
            result.Errors.Add("Email deve ter um formato válido");
        }
        else
        {
            // Verificar se email já existe para outro usuário
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null && existingUser.Id != id)
            {
                result.Errors.Add("Email já está em uso por outro usuário");
            }
        }

        result.IsValid = !result.Errors.Any();
        
        if (!result.IsValid)
        {
            _logger.LogWarning("Validação de atualização de usuário falhou: {Errors}", string.Join(", ", result.Errors));
        }

        return result;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

// Factory para criação de usuários
public class UserFactory : IUserFactory
{
    private readonly ILogger<UserFactory> _logger;

    public UserFactory(ILogger<UserFactory> logger)
    {
        _logger = logger;
    }

    public User CreateUser(string name, string email)
    {
        _logger.LogDebug("Criando usuário via factory: {Name}, {Email}", name, email);
        
        return new User
        {
            Name = name,
            Email = email,
            CreatedAt = DateTime.Now,
            IsActive = true
        };
    }

    public User CreateUser(UserDto dto)
    {
        _logger.LogDebug("Criando usuário via factory com DTO: {Name}, {Email}", dto.Name, dto.Email);
        
        return CreateUser(dto.Name, dto.Email);
    }
}
