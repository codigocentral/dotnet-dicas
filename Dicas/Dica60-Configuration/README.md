# Dica 60: Configuration & Options Pattern

## 📋 Visão Geral

Esta dica demonstra como usar o sistema de configuração do .NET e o padrão Options para gerenciar configurações de aplicações de forma robusta e tipada.

## 🎯 Conceitos Demonstrados

### 1. Fontes de Configuração

O .NET suporta múltiplas fontes de configuração com precedência bem definida:

```
1. appsettings.json (base)
2. appsettings.{Environment}.json (específico do ambiente)
3. User Secrets (desenvolvimento)
4. Variáveis de ambiente
5. Argumentos da linha de comando
6. Configuração em memória
```

### 2. Padrão Options

Três interfaces principais para acessar configurações:

- **`IOptions<T>`**: Singleton, valor fixo durante toda a vida da aplicação
- **`IOptionsMonitor<T>`**: Permite atualizações em tempo real
- **`IOptionsSnapshot<T>`**: Scoped, atualizado a cada request/scope

### 3. Validação de Configurações

- **Data Annotations**: Validação usando atributos
- **Validação Fluente**: Usando métodos `.Validate()`
- **Validação na Inicialização**: `.ValidateOnStart()`

## 🏗️ Estrutura do Projeto

```
Models/
├── ConfigurationModels.cs     # Modelos tipados para configurações

Services/
├── ConfigurationServices.cs   # Serviços usando IOptions
├── ConfigurationDemoService.cs # Orquestração das demonstrações
└── ConfigurationValidationService.cs # Validação de configurações

appsettings.json               # Configuração base
appsettings.Development.json   # Configuração de desenvolvimento
custom-config.json            # Configuração personalizada
Program.cs                    # Configuração do host e DI
```

## 💡 Principais Recursos

### Configurações Tipadas

```csharp
public class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";
    
    [Required]
    public string Provider { get; set; } = string.Empty;
    
    [Range(1, 300)]
    public int ConnectionTimeout { get; set; } = 30;
    
    public bool EnableRetryOnFailure { get; set; } = true;
}
```

### Registro com Validação

```csharp
services.AddOptions<DatabaseSettings>()
    .Bind(configuration.GetSection(DatabaseSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### Uso em Serviços

```csharp
public class DatabaseService
{
    private readonly DatabaseSettings _settings;
    
    public DatabaseService(IOptions<DatabaseSettings> options)
    {
        _settings = options.Value;
    }
}
```

### Monitoramento de Mudanças

```csharp
public class CacheService
{
    private readonly IOptionsMonitor<CacheSettings> _optionsMonitor;
    
    public CacheService(IOptionsMonitor<CacheSettings> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
        _optionsMonitor.OnChange(OnSettingsChanged);
    }
}
```

## 🚀 Como Executar

```bash
# Navegar para o diretório da dica
cd Dicas/Dica60-Configuration

# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run

# Executar com variáveis de ambiente
DICA60_FeatureFlags__EnableNewDashboard=true dotnet run

# Executar com argumentos da linha de comando
dotnet run --FeatureFlags:EnableBetaFeatures=true
```

## 🧪 Cenários de Teste

### 1. Configuração Básica
- Carregamento de configurações de múltiplas fontes
- Validação automática na inicialização
- Acesso tipado às configurações

### 2. Precedência de Configuração
- Teste com variáveis de ambiente
- Sobrescrita com argumentos da linha de comando
- Configurações específicas do ambiente

### 3. Atualização em Tempo Real
- Modificação de arquivos JSON durante execução
- Callback automático para mudanças
- Reconfiguração de serviços

### 4. Validação Avançada
- Data Annotations
- Validação customizada
- Tratamento de erros de configuração

## 📚 Conceitos Técnicos

### Options Pattern
- **Singleton**: Uma instância durante toda a aplicação
- **Scoped**: Uma instância por request/scope
- **Transient**: Nova instância a cada injeção

### Configuration Providers
- **JSON Provider**: Arquivos appsettings.json
- **Environment Variables Provider**: Variáveis do sistema
- **Command Line Provider**: Argumentos da aplicação
- **Memory Provider**: Configuração em memória
- **User Secrets Provider**: Secrets do desenvolvedor

### Validation
- **Fail Fast**: Validação na inicialização
- **Runtime Validation**: Validação durante execução
- **Custom Validators**: Validação personalizada

## 🎨 Boas Práticas

### 1. Estruturação
- Use constantes para nomes de seções
- Agrupe configurações relacionadas
- Defina valores padrão sensatos

### 2. Validação
- Sempre valide configurações críticas
- Use `ValidateOnStart()` para falha rápida
- Implemente validação customizada quando necessário

### 3. Segurança
- Nunca exponha secrets em logs
- Use User Secrets em desenvolvimento
- Configure variáveis de ambiente em produção

### 4. Performance
- Use `IOptions<T>` para configurações estáticas
- Use `IOptionsMonitor<T>` apenas quando necessário
- Evite validação excessiva em hot paths

## 🔍 Pontos de Atenção

1. **Precedência**: Entenda a ordem de precedência das fontes
2. **Recarregamento**: Nem todos os providers suportam reload
3. **Validação**: Falhas de validação podem quebrar a aplicação
4. **Threading**: `IOptionsMonitor<T>` é thread-safe
5. **Memory**: Configurações ficam em memória durante execução

## 📖 Referências

- [Configuration in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration)
- [Options Pattern](https://docs.microsoft.com/en-us/dotnet/core/extensions/options)
- [Configuration Providers](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration-providers)
- [User Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
