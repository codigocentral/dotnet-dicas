# 🎯 Dica 50: FluentValidation - Validação Robusta e Elegante

## 📋 Visão Geral

Esta dica demonstra como usar **FluentValidation** para criar validações robustas, elegantes e maintíveis em aplicações .NET, incluindo:

- ✅ **Validações Básicas** - NotEmpty, Length, Range, EmailAddress, etc.
- ✅ **Validações Condicionais** - Validação baseada em condições (`When`)
- ✅ **Validações Assíncronas** - `MustAsync` para consultas ao banco
- ✅ **Validações de Objetos Aninhados** - `SetValidator` para composição
- ✅ **Validações de Coleções** - `RuleForEach` para arrays/listas
- ✅ **Cross-Field Validation** - Validação entre múltiplos campos
- ✅ **Regras de Negócio Customizadas** - Validators específicos do domínio
- ✅ **Integração ASP.NET Core** - Pipeline automático de validação
- ✅ **Mensagens Personalizadas** - Controle total sobre mensagens de erro

## 🎯 Objetivos de Aprendizado

### **1. Fundamentos do FluentValidation**
- Configurar FluentValidation em projetos .NET
- Criar validators básicos e avançados
- Entender a sintaxe fluent e suas vantagens
- Configurar dependency injection

### **2. Padrões de Validação Avançados**
- Implementar validações condicionais
- Criar validators para objetos complexos
- Validar coleções e arrays
- Implementar cross-field validation

### **3. Integração com ASP.NET Core**
- Configurar validação automática
- Personalizar respostas de erro
- Integrar com controllers e minimal APIs
- Implementar validators assíncronos

### **4. Boas Práticas**
- Organizar validators por responsabilidade
- Criar validators reutilizáveis
- Implementar regras de negócio
- Otimizar performance de validação

## 🏗️ Estrutura do Projeto

```
Dica50-FluentValidation/
├── Program.cs                          # Configuração e demonstrações
├── Models/
│   └── Models.cs                       # Entidades e DTOs
├── Validators/
│   ├── BasicValidators.cs              # Validators básicos
│   └── AdvancedValidators.cs           # Validators avançados
├── Controllers/
│   └── ApiControllers.cs               # Controllers com validação
├── Services/
│   └── ValidationServices.cs          # Serviços para validações assíncronas
└── README.md                          # Esta documentação
```

## 📦 Dependências

```xml
<PackageReference Include="FluentValidation" Version="11.9.2" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.2" />
```

## 🚀 Como Executar

```bash
# Navegar para o diretório
cd Dicas/Dica50-FluentValidation

# Restaurar dependências
dotnet restore

# Executar aplicação
dotnet run

# Acessar a documentação
# http://localhost:5000/swagger
```

## 📚 Exemplos Práticos

### **1. Validator Básico**

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .Length(2, 100).WithMessage("Nome deve ter entre 2 e 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter formato válido");

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 120).WithMessage("Idade deve estar entre 0 e 120 anos");
    }
}
```

### **2. Validação Condicional**

```csharp
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        // Peso obrigatório apenas para produtos físicos
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Peso deve ser maior que zero")
            .When(x => !x.IsDigital);

        // Produtos digitais não devem ter peso
        RuleFor(x => x.Weight)
            .Empty().WithMessage("Produtos digitais não devem ter peso")
            .When(x => x.IsDigital);
    }
}
```

### **3. Validação Assíncrona**

```csharp
public class CompanyValidator : AbstractValidator<Company>
{
    private readonly ICompanyValidationService _service;

    public CompanyValidator(ICompanyValidationService service)
    {
        _service = service;

        RuleFor(x => x.Name)
            .MustAsync(BeUniqueCompanyName)
            .WithMessage("Nome da empresa já existe");
    }

    private async Task<bool> BeUniqueCompanyName(string name, CancellationToken cancellationToken)
    {
        return await _service.IsCompanyNameUniqueAsync(name, cancellationToken);
    }
}
```

### **4. Validação de Objetos Aninhados**

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        // Validar endereço usando validator específico
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator())
            .When(x => x.Address != null);
    }
}
```

### **5. Validação de Coleções**

```csharp
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        // Validar cada review na lista
        RuleForEach(x => x.Reviews)
            .SetValidator(new ProductReviewValidator());

        // Validação da coleção como um todo
        RuleFor(x => x.Reviews)
            .Must(reviews => reviews.All(r => r.Rating >= 1 && r.Rating <= 5))
            .WithMessage("Todas as avaliações devem ter rating entre 1 e 5");
    }
}
```

### **6. Cross-Field Validation**

```csharp
public class CrossFieldValidator : AbstractValidator<User>
{
    public CrossFieldValidator()
    {
        // Validação entre múltiplos campos
        RuleFor(x => x)
            .Must(user => CalculateAge(user.BirthDate) == user.Age)
            .WithMessage("Idade deve ser consistente com a data de nascimento");

        RuleFor(x => x)
            .Must(user => user.Age >= 18 || user.UserType == UserType.Guest)
            .WithMessage("Menores de 18 anos só podem ser usuários Guest");
    }
}
```

### **7. Regras de Negócio**

```csharp
public class BusinessRuleValidator : AbstractValidator<User>
{
    public BusinessRuleValidator()
    {
        // Usuários Premium devem ter mais de 18 anos
        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Usuários Premium devem ter pelo menos 18 anos")
            .When(x => x.UserType == UserType.Premium);

        // Administradores devem ter email corporativo
        RuleFor(x => x.Email)
            .Must(email => email.EndsWith("@company.com"))
            .WithMessage("Administradores devem usar email corporativo")
            .When(x => x.UserType == UserType.Admin);
    }
}
```

## 🌐 Endpoints de Demonstração

### **Básico**
- `GET /demo/basic-validation` - Validação básica de usuário
- `GET /demo/conditional-validation` - Validação condicional de produto

### **Avançado**
- `GET /demo/async-validation` - Validação assíncrona de empresa
- `GET /demo/business-rules` - Regras de negócio customizadas
- `GET /demo/cross-field-validation` - Validação cross-field
- `GET /demo/complex-order` - Validação complexa de pedido

### **APIs REST**
- `POST /api/users` - Criar usuário com validação
- `PUT /api/users/{id}` - Atualizar usuário
- `GET /api/users/{id}` - Buscar usuário
- `POST /api/products` - Criar produto
- `POST /api/companies/validate` - Validar empresa
- `POST /api/orders/validate` - Validar pedido

## 📊 Exemplos de Resposta

### **Validação com Sucesso**
```json
{
  "message": "Dados válidos",
  "isValid": true
}
```

### **Validação com Erros**
```json
{
  "message": "Dados inválidos",
  "isValid": false,
  "errors": {
    "Name": ["Nome é obrigatório"],
    "Email": ["Email deve ter formato válido"],
    "Age": ["Idade deve estar entre 0 e 120 anos"]
  }
}
```

## 🎯 Cenários de Teste

### **1. Validação Básica**
```bash
curl -X GET "http://localhost:5000/demo/basic-validation"
```

### **2. Validação Condicional**
```bash
curl -X GET "http://localhost:5000/demo/conditional-validation"
```

### **3. Criar Usuário Válido**
```bash
curl -X POST "http://localhost:5000/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao@email.com",
    "age": 30,
    "userType": 1
  }'
```

### **4. Criar Usuário Inválido**
```bash
curl -X POST "http://localhost:5000/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "email": "email-invalido",
    "age": -5
  }'
```

## 💡 Vantagens do FluentValidation

### **1. Sintaxe Fluent e Legível**
- API intuitiva e expressiva
- Fácil de ler e manter
- IntelliSense completo

### **2. Separação de Responsabilidades**
- Validators dedicados
- Reutilização de regras
- Testabilidade individual

### **3. Flexibilidade**
- Validações condicionais
- Validações customizadas
- Composição de validators

### **4. Integração Nativa**
- ASP.NET Core integration
- Dependency Injection
- Minimal APIs support

### **5. Performance**
- Lazy evaluation
- Short-circuit evaluation
- Async validation support

## ⚠️ Boas Práticas

### **✅ Faça**
- Use validators específicos para cada cenário
- Implemente validações assíncronas quando necessário
- Configure mensagens de erro personalizadas
- Teste validators isoladamente
- Use `When()` para validações condicionais

### **❌ Evite**
- Validators muito complexos
- Lógica de negócio nos validators
- Validações síncronas desnecessárias
- Mensagens de erro genéricas
- Validators aninhados muito profundos

## 🔧 Configuração Avançada

### **Dependency Injection**
```csharp
// Registrar todos os validators do assembly
services.AddValidatorsFromAssemblyContaining<UserValidator>();

// Registrar validators específicos
services.AddScoped<IValidator<User>, UserValidator>();
```

### **Configuração Global**
```csharp
ValidatorOptions.Global.LanguageManager.Enabled = false;
ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) => 
    member?.Name;
```

## 📈 Métricas e Monitoramento

O projeto inclui logging detalhado para:
- ✅ Tempo de validação
- ✅ Validators executados
- ✅ Errors e warnings
- ✅ Performance de validações assíncronas

## 🎓 Conceitos Demonstrados

1. **Validators Básicos** - NotEmpty, Length, Range, EmailAddress
2. **Validators Condicionais** - When, Unless, ApplyConditionTo
3. **Validators Assíncronos** - MustAsync para consultas
4. **Composição** - SetValidator para objetos aninhados
5. **Coleções** - RuleForEach para arrays/listas
6. **Cross-Field** - Validação entre múltiplos campos
7. **Regras de Negócio** - Validators específicos do domínio
8. **Integração** - ASP.NET Core pipeline
9. **Performance** - Otimizações e async patterns
10. **Testabilidade** - Validators isolados e testáveis

## 🔍 Recursos Adicionais

- [Documentação Oficial](https://docs.fluentvalidation.net/)
- [Exemplos no GitHub](https://github.com/FluentValidation/FluentValidation)
- [Validators Built-in](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)

---

**FluentValidation oferece uma abordagem elegante e poderosa para validação em .NET, proporcionando código limpo, maintível e altamente testável! 🎯**
