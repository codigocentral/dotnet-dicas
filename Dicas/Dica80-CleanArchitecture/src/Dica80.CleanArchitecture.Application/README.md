# 🚀 Application Layer - Clean Architecture

## 📋 Visão Geral

A **Application Layer** implementa os casos de uso da aplicação usando **CQRS** (Command Query Responsibility Segregation) com **MediatR**:

- ✅ **Commands** - Operações que modificam estado
- ✅ **Queries** - Operações que consultam dados  
- ✅ **Handlers** - Processam commands e queries
- ✅ **DTOs** - Transferência de dados entre camadas
- ✅ **Validators** - Validação com FluentValidation
- ✅ **Behaviors** - Cross-cutting concerns (logging, performance)
- ✅ **Event Handlers** - Processam domain events
- ✅ **AutoMapper** - Mapeamento entre entidades e DTOs

## 🎯 Padrões Implementados

### **1. CQRS (Command Query Responsibility Segregation)**
```csharp
// Commands - Modificam estado
public record CreateUserCommand : BaseCommand<Result<UserDto>>
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

// Queries - Consultam dados
public record GetUserByIdQuery : BaseQuery<Result<UserDto>>
{
    public Guid Id { get; init; }
}
```

### **2. MediatR Pipeline**
```csharp
Request → ValidationBehavior → LoggingBehavior → PerformanceBehavior → Handler
```

### **3. Result Pattern**
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public List<string> ValidationErrors { get; }
}
```

## 📁 Estrutura

```
Application/
├── Common/
│   ├── BaseHandlers.cs         # Classes base para handlers
│   ├── ValidationBehavior.cs   # Pipeline de validação
│   └── Behaviors.cs            # Logging e performance
├── DTOs/
│   ├── UserAndProjectDTOs.cs   # DTOs de usuário/projeto
│   └── TaskAndCommentDTOs.cs   # DTOs de tarefa/comentário
├── Users/
│   ├── Commands/
│   │   └── UserCommands.cs     # Commands de usuário
│   └── Queries/
│       └── UserQueries.cs      # Queries de usuário
├── Mappings/
│   └── ApplicationMappingProfile.cs # Mapeamentos AutoMapper
├── EventHandlers/
│   └── DomainEventHandlers.cs  # Handlers de domain events
└── DependencyInjection.cs      # Configuração DI
```

## ⚡ Principais Características

### **Commands com Validação**
```csharp
public class CreateUserCommandValidator : BaseValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        ValidateRequiredEmail(x => x.Email);
        ValidateRequiredString(nameof(CreateUserCommand.Name), x => x.Name);
        ValidateEnum(nameof(CreateUserCommand.Role), x => x.Role);
    }
}
```

### **Handlers com Result Pattern**
```csharp
public override async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    try
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return Result<UserDto>.Failure("User with this email already exists");

        var email = Email.Create(request.Email);
        var user = User.Create(email, request.Name, request.Role);
        
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = Mapper.Map<UserDto>(user);
        return Result<UserDto>.Success(userDto);
    }
    catch (Exception ex)
    {
        return Result<UserDto>.Failure($"Failed to create user: {ex.Message}");
    }
}
```

### **Queries com Paginação**
```csharp
public record GetUsersQuery : BaseQuery<Result<PagedResult<UserDto>>>
{
    public PaginationParams Pagination { get; init; } = new();
    public string? SearchTerm { get; init; }
    public UserRole? Role { get; init; }
    public bool? IsActive { get; init; }
}
```

### **Domain Event Handlers**
```csharp
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Send welcome email
        // Create user profile
        // Initialize user settings
    }
}
```

## 🔧 Pipeline Behaviors

### **1. Validation Behavior**
- Executa **antes** do handler
- Usa **FluentValidation** para validar requests
- Retorna **Result.ValidationFailure** se inválido

### **2. Logging Behavior**
- Loga **início** e **fim** de cada request
- Registra **tempo de execução**
- Captura **exceções** com contexto

### **3. Performance Behavior**
- Monitora requests **lentos** (>1000ms)
- Gera **warnings** para otimização
- Facilita identificação de **gargalos**

## 📊 AutoMapper Configuration

```csharp
CreateMap<User, UserDto>()
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));

CreateMap<Project, ProjectDto>()
    .ForMember(dest => dest.Budget, opt => opt.MapFrom(src => src.Budget != null ? src.Budget.Amount : (decimal?)null))
    .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count));
```

## 🎨 Benefícios do CQRS

✅ **Separação de Responsabilidades**: Commands vs Queries  
✅ **Otimização Independente**: Diferentes estratégias para leitura/escrita  
✅ **Escalabilidade**: Pode usar diferentes databases  
✅ **Flexibilidade**: DTOs específicos para cada operação  
✅ **Testabilidade**: Handlers isolados e focados  
✅ **Performance**: Queries otimizadas para exibição  

## 🛡️ Tratamento de Erros

- **Result Pattern** para sucesso/falha
- **Validação centralizada** com FluentValidation
- **Logging estruturado** para debugging
- **Exceções específicas** por contexto
- **Rollback automático** em caso de erro

## 🔄 Fluxo de Execução

1. **Controller** recebe request HTTP
2. **Controller** cria Command/Query
3. **MediatR** executa pipeline behaviors
4. **ValidationBehavior** valida request
5. **LoggingBehavior** loga operação
6. **Handler** processa lógica de negócio
7. **Repository** persiste dados (Commands)
8. **Domain Events** são publicados
9. **Event Handlers** processam eventos
10. **Result** é retornado ao controller

---

💡 **Esta camada orquestra os casos de uso sem conter regras de negócio!**
