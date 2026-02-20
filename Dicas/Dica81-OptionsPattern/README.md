# 🎯 Dica 81: Options Pattern e Configuration em .NET

## 📋 Visão Geral

Esta dica demonstra como usar o **Options Pattern** em .NET para gerenciar configurações de forma **tipada**, **validada** e **flexível**. Aborda as diferenças entre `IOptions<T>`, `IOptionsSnapshot<T>` e `IOptionsMonitor<T>`, além de técnicas avançadas de validação e configuração.

## 🎯 Objetivos

- ✅ Entender os três tipos de Options Pattern
- ✅ Configurar validação automática com DataAnnotations
- ✅ Implementar validação customizada
- ✅ Trabalhar com configurações complexas aninhadas
- ✅ Demonstrar recarga de configurações em tempo real
- ✅ Aplicar boas práticas de segurança

## 🔍 Tipos de Options Pattern

### 1. 📌 IOptions\<T\> (Singleton)
```csharp
public DatabaseService(IOptions<DatabaseSettings> options)
{
    _settings = options.Value; // Uma vez por aplicação
}
```
- **Lifetime**: Singleton
- **Mudanças**: ❌ Não reflete mudanças
- **Uso**: Configurações estáticas

### 2. 📌 IOptionsSnapshot\<T\> (Scoped)
```csharp
public EmailService(IOptionsSnapshot<EmailSettings> optionsSnapshot)
{
    var settings = optionsSnapshot.Value; // Nova a cada request
}
```
- **Lifetime**: Scoped
- **Mudanças**: ✅ Reflete mudanças por request
- **Uso**: Configurações por tenant

### 3. 📌 IOptionsMonitor\<T\> (Singleton com Reload)
```csharp
public ApiService(IOptionsMonitor<ApiSettings> optionsMonitor)
{
    var settings = optionsMonitor.CurrentValue; // Sempre atual
    
    // Monitora mudanças
    optionsMonitor.OnChange(OnSettingsChanged);
}
```
- **Lifetime**: Singleton
- **Mudanças**: ✅ Reflete mudanças em tempo real
- **Uso**: Feature flags, configurações dinâmicas

## 🛡️ Validação de Configurações

### DataAnnotations Básicas
```csharp
public class DatabaseSettings
{
    [Required(ErrorMessage = "ConnectionString é obrigatória")]
    [MinLength(10)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 300)]
    public int CommandTimeout { get; set; } = 30;
}
```

### Validação Customizada
```csharp
[CustomValidation(typeof(CustomValidatedSettings), nameof(ValidateEnvironment))]
public string Environment { get; set; } = "Development";

public static ValidationResult? ValidateEnvironment(string environment, ValidationContext context)
{
    var validEnvironments = new[] { "Development", "Staging", "Production" };
    
    if (!validEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase))
    {
        return new ValidationResult($"Environment deve ser: {string.Join(", ", validEnvironments)}");
    }
    
    return ValidationResult.Success;
}
```

### Validação Complexa com IValidateOptions
```csharp
public class CacheSettingsValidator : IValidateOptions<CacheSettings>
{
    public ValidateOptionsResult Validate(string? name, CacheSettings options)
    {
        var failures = new List<string>();

        if (options.MaxMemoryMB > 512)
        {
            failures.Add("MaxMemoryMB não deve exceder 512MB");
        }

        return failures.Count > 0 
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
```

## 🔧 Configuração na Startup

```csharp
// Configuração básica
services.Configure<DatabaseSettings>(
    configuration.GetSection("DatabaseSettings"));

// Com validação automática
services.AddOptions<EmailSettings>()
    .Bind(configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Com validação customizada
services.AddOptions<ApiSettings>()
    .Bind(configuration.GetSection("ApiSettings"))
    .ValidateDataAnnotations()
    .Validate(settings => settings.TimeoutInSeconds <= 60, 
              "Timeout não deve exceder 60 segundos")
    .ValidateOnStart();

// Post-configure para modificações
services.PostConfigure<ApiSettings>(settings =>
{
    if (settings.BaseUrl.StartsWith("http://"))
    {
        settings.BaseUrl = settings.BaseUrl.Replace("http://", "https://");
    }
});
```

## 📁 Estrutura do Projeto

```
Dica81-OptionsPattern/
├── Configuration/
│   └── Settings.cs              # Classes de configuração tipadas
├── Services/
│   ├── ConfigurationServices.cs # Serviços demonstrando IOptions
│   ├── ValidationService.cs     # Validação manual
│   └── DemoHostedService.cs     # Orquestração da demo
├── appsettings.json             # Arquivo de configuração
├── Program.cs                   # Configuração da aplicação
└── README.md                    # Esta documentação
```

## 🎨 Características Demonstradas

### ✅ Configurações Tipadas
- Classes C# para representar configurações
- Binding automático do JSON para objetos
- IntelliSense e verificação de tipos

### ✅ Validação Robusta
- Validação com DataAnnotations
- Validação customizada complexa
- Falha rápida na inicialização

### ✅ Configurações Aninhadas
- Objetos complexos com sub-propriedades
- Arrays e listas de configurações
- Estruturas hierárquicas

### ✅ Segurança
- Mascaramento de dados sensíveis nos logs
- Configurações por ambiente
- Validação de URLs e formatos

### ✅ Monitoramento de Mudanças
- Recarga automática de configurações
- Callbacks para mudanças
- Logs de alterações

## 🚀 Como Executar

```bash
# 1. Navegar para o diretório
cd Dicas/Dica81-OptionsPattern

# 2. Restaurar dependências
dotnet restore

# 3. Executar a demonstração
dotnet run
```

## 📊 Saída Esperada

```
🎯 Dica 81: Options Pattern e Configuration em .NET
====================================================
🚀 Iniciando demonstração do Options Pattern
============================================================

📖 1. Demonstrando IOptions<T> (Singleton)
--------------------------------------------------
Configuração do Banco de Dados (IOptions - Singleton):
- Connection: Server=loc***ty=true;
- Timeout: 30s
- Retry: Habilitado
- Max Retries: 3

📧 2. Demonstrando IOptionsSnapshot<T> (Scoped)
--------------------------------------------------
Email Enviado (IOptionsSnapshot - Scoped):
- Para: usuario@exemplo.com
- Assunto: Teste do Options Pattern
- SMTP: smtp.exemplo.com:587
- SSL: Habilitado
- De: Sistema Exemplo <noreply@exemplo.com>

🌐 3. Demonstrando IOptionsMonitor<T> (Singleton com reload)
--------------------------------------------------
Chamada de API (IOptionsMonitor - Singleton com reload):
- URL: https://api.exemplo.com/users/123
- Timeout: 30s
- API Key: secr***123
- Features:
  Caching: ✅
  Logging: ✅
  Retries: ❌

💾 4. Demonstrando Configurações Complexas Aninhadas
--------------------------------------------------
Configuração do Cache (Objetos Aninhados):
- Expiração Padrão: 15 minutos
- Memória Máxima: 100 MB
- Sliding Expiration: Habilitado
- Providers Habilitados (1):
   1. Memory

🔍 5. Demonstrando Validação de Configurações
--------------------------------------------------
✅ Todas as configurações são válidas!
```

## 🌟 Boas Práticas Demonstradas

### 1. 📝 Convenções de Nomenclatura
- Sufixo "Settings" nas classes
- Propriedade `SectionName` constante
- Nomes descritivos e claros

### 2. 🛡️ Validação Abrangente
- Validação na inicialização com `ValidateOnStart()`
- Combinação de DataAnnotations e validação customizada
- Mensagens de erro claras e específicas

### 3. 🔒 Segurança
- Mascaramento de dados sensíveis
- Validação de formatos (URLs, emails)
- Configurações por ambiente

### 4. 🏗️ Estrutura Organizacional
- Separação por responsabilidade
- Configurações aninhadas para complexidade
- Reutilização de configurações comuns

### 5. ⚡ Performance
- Escolha correta do tipo de Options
- Evitar re-validação desnecessária
- Cache de configurações quando apropriado

## 💡 Casos de Uso Reais

### 🏢 Aplicações Enterprise
- Configurações de banco de dados
- Configurações de email/SMTP
- URLs de APIs externas
- Configurações de cache

### 🌐 Aplicações Multi-Tenant
- Configurações específicas por tenant
- Features flags dinâmicas
- Temas e personalizações

### 🔧 Microserviços
- Descoberta de serviços
- Configurações de circuit breaker
- Limites de rate limiting
- Configurações de observabilidade

### ☁️ Cloud Native
- Configurações por ambiente
- Secrets e chaves de API
- Configurações de escalonamento
- Configurações de monitoramento

## 🎓 Conceitos Abordados

- **Options Pattern**: Gerenciamento tipado de configurações
- **Dependency Injection**: Injeção de configurações
- **Data Annotations**: Validação declarativa
- **Configuration Binding**: Mapeamento JSON → Objetos
- **Hot Reload**: Recarga de configurações em runtime
- **Validation**: Validação robusta e customizada
- **Security**: Práticas seguras para configurações

---

💡 **Esta dica é essencial para qualquer aplicação .NET que precisa de configurações robustas, tipadas e validadas!**
