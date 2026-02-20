# 🏗️ **Dica 80: Clean Architecture com ASP.NET Core**

> **Demonstração completa de Clean Architecture seguindo os princípios de Robert C. Martin**

---

## 🎯 **Objetivo**

Esta dica demonstra como implementar **Clean Architecture** em .NET 9.0, seguindo as melhores práticas de desenvolvimento enterprise com:

- ✅ **Separação de responsabilidades** em 4 camadas bem definidas
- ✅ **Inversão de dependências** entre todas as camadas
- ✅ **CQRS** (Command Query Responsibility Segregation) com MediatR
- ✅ **Domain-Driven Design** (DDD) com entidades ricas
- ✅ **Domain Events** para comunicação assíncrona
- ✅ **Repository Pattern** com Unit of Work
- ✅ **Entity Framework Core** com configurações avançadas
- ✅ **API RESTful** com documentação Swagger
- ✅ **Testes unitários** e de integração
- ✅ **Logging estruturado** com Serilog

---

## 🏛️ **Arquitetura**

```
┌─────────────────────────────────────────────────────────────┐
│                        WebAPI Layer                         │
│  Controllers, Middleware, Configuration, Swagger           │
└─────────────────────┬───────────────────────────────────────┘
                      │ (depends on)
┌─────────────────────▼───────────────────────────────────────┐
│                   Application Layer                         │
│    CQRS, Handlers, DTOs, Validation, Mapping              │
└─────────────────────┬───────────────────────────────────────┘
                      │ (depends on)
┌─────────────────────▼───────────────────────────────────────┐
│                    Domain Layer                             │
│   Entities, Value Objects, Events, Repository Interfaces   │
└─────────────────────▲───────────────────────────────────────┘
                      │ (implements)
┌─────────────────────┴───────────────────────────────────────┐
│                 Infrastructure Layer                        │
│   EF Core, Repositories, External Services, Database       │
└─────────────────────────────────────────────────────────────┘
```

---

## 📁 **Estrutura do Projeto**

```
Dica80-CleanArchitecture/
├── src/
│   ├── Dica80.CleanArchitecture.Domain/         # 🏛️ Domain Layer
│   │   ├── Common/BaseEntity.cs                 # Base para entidades
│   │   ├── Entities/                            # User, Project, Task, Comment
│   │   ├── ValueObjects/                        # Email, Money, Priority
│   │   ├── Events/                              # Domain Events
│   │   ├── Repositories/                        # Interfaces
│   │   └── Exceptions/                          # Domain Exceptions
│   │
│   ├── Dica80.CleanArchitecture.Application/    # 📋 Application Layer
│   │   ├── Common/                              # CQRS Base Classes
│   │   ├── DTOs/                                # Data Transfer Objects
│   │   ├── Users/Commands|Queries/              # User Operations
│   │   ├── Projects/Commands|Queries/           # Project Operations
│   │   ├── Mappings/                            # AutoMapper Profiles
│   │   └── EventHandlers/                       # Domain Event Handlers
│   │
│   ├── Dica80.CleanArchitecture.Infrastructure/ # 🔧 Infrastructure Layer
│   │   ├── Data/                                # EF Core DbContext
│   │   ├── Configurations/                      # Entity Configurations
│   │   ├── Repositories/                        # Repository Implementations
│   │   └── Services/                            # External Services
│   │
│   └── Dica80.CleanArchitecture.WebAPI/         # 🌐 WebAPI Layer
│       ├── Controllers/                         # API Controllers
│       ├── Middleware/                          # Custom Middleware
│       ├── Extensions/                          # Extension Methods
│       └── Program.cs                           # App Configuration
│
├── tests/
│   └── Dica80.CleanArchitecture.Tests/          # 🧪 Unit & Integration Tests
│       ├── Domain/                              # Domain Tests
│       ├── Application/                         # Application Tests
│       └── Integration/                         # API Tests
│
├── README.md                                    # Este arquivo
├── PROGRESS.md                                  # Status da implementação
└── Dica80-CleanArchitecture.sln                # Solution file
```

---

## ⚡ **Tecnologias Utilizadas**

| Camada | Tecnologias | Responsabilidade |
|--------|-------------|------------------|
| **Domain** | `.NET 9.0` (sem dependências) | Regras de negócio, entidades |
| **Application** | `MediatR`, `FluentValidation`, `AutoMapper` | Casos de uso, orquestração |
| **Infrastructure** | `Entity Framework Core`, `SQL Server` | Persistência, serviços externos |
| **WebAPI** | `ASP.NET Core`, `Swagger`, `Serilog` | Controllers, middleware, logs |
| **Tests** | `xUnit`, `FluentAssertions`, `Moq` | Testes unitários e integração |

---

## 🚀 **Como Executar**

### **Pré-requisitos**
- .NET 9.0 SDK
- SQL Server (opcional - usa In-Memory por padrão)
- Visual Studio 2022 ou VS Code

### **Passos**

1. **Clone o repositório**
   ```bash
   git clone <repository-url>
   cd didaticos-dicas-csharp/Dicas/Dica80-CleanArchitecture
   ```

2. **Restaure as dependências**
   ```bash
   dotnet restore
   ```

3. **Execute a aplicação**
   ```bash
   cd src/Dica80.CleanArchitecture.WebAPI
   dotnet run
   ```

4. **Acesse a API**
   - Swagger UI: `https://localhost:5001`
   - Health Check: `https://localhost:5001/health`

5. **Execute os testes**
   ```bash
   cd tests/Dica80.CleanArchitecture.Tests
   dotnet test
   ```

---

## 🎨 **Exemplos de Uso**

### **1. Criar Usuário**
```bash
POST /api/users
{
  "email": "john.doe@example.com",
  "name": "John Doe",
  "role": "Member"
}
```

### **2. Buscar Usuários**
```bash
GET /api/users?pageNumber=1&pageSize=10&searchTerm=john&isActive=true
```

### **3. Criar Projeto**
```bash
POST /api/projects
{
  "name": "Sistema de Vendas",
  "description": "Sistema completo de vendas online",
  "ownerId": "guid-do-usuario",
  "startDate": "2025-01-01",
  "endDate": "2025-06-30",
  "budgetAmount": 50000,
  "budgetCurrency": "USD"
}
```

---

## 🏗️ **Conceitos Demonstrados**

### **1. Domain Layer (Núcleo)**
```csharp
// Entidade com comportamento rico
public class User : BaseEntity
{
    public void Activate() => IsActive = true;
    public void UpdateLastLogin() => LastLoginAt = DateTime.UtcNow;
}

// Value Object validado
public class Email : ValueObject
{
    public static Email Create(string email)
    {
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");
        return new Email(email.ToLowerInvariant());
    }
}

// Domain Event
public class UserCreatedEvent : BaseDomainEvent
{
    public User User { get; }
    public UserCreatedEvent(User user) => User = user;
}
```

### **2. Application Layer (CQRS)**
```csharp
// Command
public record CreateUserCommand : BaseCommand<Result<UserDto>>
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

// Query
public record GetUserByIdQuery : BaseQuery<Result<UserDto>>
{
    public Guid Id { get; init; }
}

// Handler
public class CreateUserCommandHandler : BaseCommandHandler<CreateUserCommand, Result<UserDto>>
{
    public override async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validação, criação, persistência
        return Result<UserDto>.Success(userDto);
    }
}
```

### **3. Infrastructure Layer (Persistência)**
```csharp
// Entity Configuration
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email");
        });
    }
}

// Repository Implementation
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet.Where(u => u.Email.Value == email).FirstOrDefaultAsync();
    }
}
```

### **4. WebAPI Layer (Interface)**
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto request)
    {
        var command = new CreateUserCommand { Email = request.Email, Name = request.Name };
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetUser), new { id = result.Data?.Id });
    }
}
```

---

## 📊 **Benefícios Alcançados**

| Aspecto | Benefício | Como Alcançado |
|---------|-----------|----------------|
| **Manutenibilidade** | Código organizado e legível | Separação clara de responsabilidades |
| **Testabilidade** | Cobertura de testes alta | Dependency injection e abstrações |
| **Escalabilidade** | Fácil adicionar funcionalidades | CQRS e event-driven architecture |
| **Flexibilidade** | Mudança de tecnologias | Dependency inversion principle |
| **Qualidade** | Bugs reduzidos | Validações e tratamento de erros |
| **Performance** | Queries otimizadas | CQRS com queries específicas |

---

## 🧪 **Testes Incluídos**

### **Domain Tests**
- ✅ Testes de entidades e regras de negócio
- ✅ Testes de value objects e validações
- ✅ Testes de domain events

### **Application Tests**
- ✅ Testes de command handlers
- ✅ Testes de query handlers
- ✅ Testes de validation behaviors

### **Integration Tests**
- ✅ Testes de API endpoints
- ✅ Testes de persistência
- ✅ Testes de pipeline completo

---

## 🔍 **Exemplos de Teste**

```csharp
[Fact]
public void Create_ValidInput_ShouldCreateUser()
{
    // Arrange
    var email = Email.Create("test@example.com");
    var name = "John Doe";

    // Act
    var user = User.Create(email, name, UserRole.Member);

    // Assert
    user.Should().NotBeNull();
    user.Email.Should().Be(email);
    user.DomainEvents.Should().HaveCount(1);
    user.DomainEvents.First().Should().BeOfType<UserCreatedEvent>();
}
```

---

## 🎯 **Recursos Demonstrados**

### **Padrões de Design**
- ✅ **Repository Pattern** - Abstração de dados
- ✅ **Unit of Work** - Transações consistentes
- ✅ **CQRS** - Separação command/query
- ✅ **Domain Events** - Comunicação assíncrona
- ✅ **Result Pattern** - Tratamento de erros
- ✅ **Specification Pattern** - Regras complexas

### **Princípios SOLID**
- ✅ **Single Responsibility** - Cada classe tem um propósito
- ✅ **Open/Closed** - Extensível sem modificação
- ✅ **Liskov Substitution** - Interfaces consistentes
- ✅ **Interface Segregation** - Interfaces específicas
- ✅ **Dependency Inversion** - Abstrações vs implementações

### **Clean Code**
- ✅ **Naming** - Nomes expressivos e claros
- ✅ **Functions** - Pequenas e focadas
- ✅ **Comments** - XML documentation
- ✅ **Error Handling** - Exceções específicas
- ✅ **Testing** - Cobertura abrangente

---

## 📚 **Recursos Adicionais**

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

## 🎉 **Conclusão**

Esta implementação demonstra como construir **aplicações enterprise robustas e escaláveis** seguindo as melhores práticas da indústria:

- **Arquitetura limpa** com separação clara de responsabilidades
- **Código testável** e de alta qualidade
- **Flexibilidade** para mudanças futuras
- **Performance** otimizada com CQRS
- **Manutenibilidade** através de bons princípios

**Perfect para projetos enterprise que precisam de uma base sólida e escalável!** 🚀

---

💡 **Esta é uma implementação completa e pronta para produção seguindo as melhores práticas do mercado!**
