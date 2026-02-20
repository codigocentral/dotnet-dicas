# Dica 44: MediatR - Padrão Mediator com CQRS

## 📋 Sobre

Esta dica demonstra como implementar o **padrão Mediator** usando a biblioteca **MediatR** em C#, com foco em:

- **CQRS (Command Query Responsibility Segregation)**
- **Pipeline Behaviors** para cross-cutting concerns
- **Notifications** para pub/sub pattern
- **Validação automática** com FluentValidation
- **Logging, Performance, Cache e Retry** automáticos

## 🎯 Objetivos de Aprendizado

- Entender o padrão Mediator e sua aplicação
- Implementar CQRS separando Commands de Queries
- Utilizar Notifications para comunicação desacoplada
- Aplicar Pipeline Behaviors para funcionalidades transversais
- Configurar validação automática com FluentValidation

## 🏗️ Estrutura do Projeto

```
Dica44-MediatR/
├── Program.cs                          # Configuração e demonstrações
├── Models/
│   └── Models.cs                       # Entidades e repositório
├── Commands/
│   └── Commands.cs                     # Commands com handlers e validators
├── Queries/
│   └── Queries.cs                      # Queries com handlers e validators
├── Notifications/
│   └── Notifications.cs               # Notifications com múltiplos handlers
├── Behaviors/
│   └── PipelineBehaviors.cs           # Cross-cutting concerns
└── README.md                          # Esta documentação
```

## 🔧 Principais Componentes

### 1. Commands (Write Operations)

```csharp
// Command para criar usuário
public record CreateUserCommand(string Name, string Email) : IRequest<User>;

// Handler correspondente
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Lógica de criação
    }
}

// Validator automático
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

### 2. Queries (Read Operations)

```csharp
// Query para buscar usuários
public record SearchUsersQuery(string? SearchTerm, int Page = 1, int PageSize = 10) 
    : IRequest<SearchUsersResult>;

// Handler com paginação
public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, SearchUsersResult>
{
    public async Task<SearchUsersResult> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        // Lógica de busca com paginação
    }
}
```

### 3. Notifications (Pub/Sub)

```csharp
// Notification para eventos
public record UserCreatedNotification(User User) : INotification;

// Múltiplos handlers executam em paralelo
public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedNotification> { }
public class AuditUserCreatedHandler : INotificationHandler<UserCreatedNotification> { }
public class ExternalSystemIntegrationHandler : INotificationHandler<UserCreatedNotification> { }
```

### 4. Pipeline Behaviors

```csharp
// Logging automático para todas as requisições
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Log entrada
        var response = await next();
        // Log saída
        return response;
    }
}
```

## ⚙️ Pipeline Behaviors Implementados

1. **LoggingBehavior** - Log automático de entrada/saída
2. **ValidationBehavior** - Validação automática com FluentValidation
3. **CachingBehavior** - Cache automático para queries
4. **PerformanceBehavior** - Monitoramento de performance
5. **RetryBehavior** - Retry automático para operações específicas

## 🚀 Como Executar

```bash
# Clone o repositório
git clone [url-do-repositorio]

# Navegue até a pasta da dica
cd Dicas/Dica44-MediatR

# Execute o projeto
dotnet run
```

## 📊 Demonstrações Incluídas

### 1. Commands (Write)
- ✅ Criar usuários com validação
- ✅ Atualizar dados existentes
- ✅ Operações lentas (performance monitoring)
- ✅ Processamento em lote
- ✅ Tratamento de erros de validação

### 2. Queries (Read)
- ✅ Buscar por ID
- ✅ Listar todos os registros
- ✅ Busca com filtros e paginação
- ✅ Estatísticas e agregações
- ✅ Demonstração de cache

### 3. Notifications (Events)
- ✅ Eventos de usuário criado (múltiplos handlers)
- ✅ Eventos de usuário atualizado
- ✅ Eventos do sistema
- ✅ Monitoramento de operações

### 4. Pipeline Behaviors
- ✅ Logging automático
- ✅ Validação com FluentValidation
- ✅ Cache para queries
- ✅ Monitoramento de performance
- ✅ Retry para falhas transitórias

## 🎯 Benefícios do Padrão

### ✅ Vantagens

- **Desacoplamento**: Reduz dependências entre componentes
- **Testabilidade**: Facilita testes unitários
- **Flexibilidade**: Permite adicionar funcionalidades sem modificar código existente
- **CQRS**: Separação clara entre operações de leitura e escrita
- **Cross-cutting Concerns**: Pipeline behaviors para funcionalidades transversais
- **Pub/Sub**: Notifications permitem comunicação assíncrona

### ⚠️ Considerações

- **Complexidade**: Adiciona uma camada de abstração
- **Performance**: Overhead mínimo do mediator
- **Curva de Aprendizado**: Requer entendimento dos padrões
- **Over-engineering**: Pode ser excessivo para aplicações simples

## 🔍 Casos de Uso Ideais

- **Aplicações Enterprise**: Sistemas complexos com muitas regras de negócio
- **Clean Architecture**: Camada de Application com CQRS
- **Event-Driven Architecture**: Sistemas baseados em eventos
- **APIs RESTful**: Controllers delegando para mediator
- **Microservices**: Comunicação interna desacoplada

## 📚 Conceitos Demonstrados

- **Mediator Pattern**: Centralização de comunicação
- **CQRS**: Command Query Responsibility Segregation
- **Pipeline Pattern**: Behaviors executados em sequência
- **Observer Pattern**: Notifications com múltiplos handlers
- **Dependency Injection**: Configuração automática de serviços
- **FluentValidation**: Validação fluente e expressiva

## 🛠️ Tecnologias Utilizadas

- **.NET 9.0**: Framework principal
- **MediatR 12.4.1**: Implementação do padrão Mediator
- **FluentValidation 11.9.2**: Validação automática
- **Microsoft.Extensions.Hosting**: Dependency Injection
- **Microsoft.Extensions.Logging**: Logging automático

## 📖 Referências

- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Mediator Pattern](https://refactoring.guru/design-patterns/mediator)

---

**Próxima Dica**: Verificar DICAS_FALTANTES.md para a próxima implementação prioritária.
