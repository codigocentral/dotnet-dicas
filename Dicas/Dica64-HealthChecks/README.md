# Dica 64: Health Checks em .NET

## 📋 Sobre a Dica

Esta dica demonstra como implementar e usar **Health Checks** em aplicações .NET para monitorar a saúde da aplicação e seus componentes.

## 🎯 Conceitos Demonstrados

### 1. Health Checks Básicos
- Implementação da interface `IHealthCheck`
- Verificações síncronas e assíncronas
- Diferentes status: Healthy, Degraded, Unhealthy

### 2. Health Checks Customizados
- **MemoryHealthCheck**: Monitora uso de memória
- **DiskSpaceHealthCheck**: Verifica espaço em disco
- **ExternalServiceHealthCheck**: Testa conectividade com APIs externas
- **DatabaseConnectionHealthCheck**: Valida conexões de banco
- **ApplicationDependenciesHealthCheck**: Verifica dependências do DI

### 3. Configuração e Registro
- Registro de health checks no DI container
- Configuração de tags para categorização
- Configuração de timeouts e thresholds

### 4. Monitoramento e Relatórios
- Geração de relatórios de saúde
- Filtragem por tags
- Exportação em formato JSON e texto
- Logs estruturados para monitoramento

## 🏗️ Estrutura do Projeto

```
Dica64-HealthChecks/
├── HealthChecks/
│   └── CustomHealthChecks.cs      # Implementações customizadas
├── Models/
│   └── HealthCheckModels.cs       # DTOs e configurações
├── Services/
│   └── HealthCheckServices.cs     # Serviços de demonstração
├── Program.cs                     # Configuração e execução
├── appsettings.json              # Configurações
└── README.md
```

## 🔧 Health Checks Implementados

### 1. Memory Health Check
```csharp
public class MemoryHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        var memoryUsed = GC.GetTotalMemory(false) / (1024 * 1024); // MB
        // Verificação da memória...
    }
}
```

### 2. Disk Space Health Check
```csharp
public class DiskSpaceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        var drives = DriveInfo.GetDrives();
        // Verificação de espaço em disco...
    }
}
```

### 3. External Service Health Check
```csharp
public class ExternalServiceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // Teste de conectividade com API externa...
    }
}
```

## ⚙️ Configuração

### appsettings.json
```json
{
  "HealthChecks": {
    "DatabaseTimeout": 30000,
    "ApiTimeout": 15000,
    "DiskSpaceThresholdMB": 1000,
    "MemoryThresholdMB": 500
  },
  "ExternalServices": {
    "ApiUrl": "https://api.exemplo.com/health"
  }
}
```

### Program.cs
```csharp
// Registro de health checks
services.AddHealthChecks()
    .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "system" })
    .AddCheck<DiskSpaceHealthCheck>("disk-space", tags: new[] { "system" })
    .AddCheck<ExternalServiceHealthCheck>("external-api", tags: new[] { "external" });
```

## 🚀 Como Executar

```bash
# Navegar para o diretório
cd Dicas/Dica64-HealthChecks

# Executar a aplicação
dotnet run
```

## 📊 Recursos de Monitoramento

### 1. Verificação Básica
- Executa todos os health checks registrados
- Exibe status geral da aplicação

### 2. Filtragem por Tags
- Filtro por categoria (system, database, external)
- Verificação seletiva de componentes

### 3. Relatórios Detalhados
- Relatório em formato JSON estruturado
- Relatório em texto legível
- Informações de tempo de execução

### 4. Cenários de Demonstração
- Simulação de falhas
- Testes de timeout
- Verificação de thresholds

## 🔍 Logs e Monitoramento

A aplicação gera logs estruturados para:
- Status de cada health check
- Tempo de execução
- Dados coletados (memória, disco, etc.)
- Erros e exceções

## 📝 Casos de Uso

### 1. Aplicações Web
- Endpoint `/health` para load balancers
- Verificação de dependências externas
- Monitoramento de recursos do sistema

### 2. Microsserviços
- Health checks entre serviços
- Verificação de conectividade
- Status de bases de dados

### 3. Aplicações Críticas
- Monitoramento contínuo
- Alertas automáticos
- Relatórios de disponibilidade

## 🎓 Conceitos Importantes

### Status de Health Check
- **Healthy**: Tudo funcionando normalmente
- **Degraded**: Funcionando com limitações
- **Unhealthy**: Não funcionando

### Tags e Categorização
- Organização de health checks por categoria
- Filtragem seletiva
- Relatórios segmentados

### Timeouts e Thresholds
- Configuração de limites de tempo
- Definição de valores críticos
- Prevenção de bloqueios

## 🔧 Tecnologias Utilizadas

- **.NET 9.0**: Framework base
- **Microsoft.Extensions.Diagnostics.HealthChecks**: Sistema de health checks
- **Microsoft.Extensions.DependencyInjection**: Injeção de dependência
- **Microsoft.Extensions.Logging**: Sistema de logs
- **System.Text.Json**: Serialização JSON

## 💡 Dicas de Implementação

1. **Performance**: Health checks devem ser rápidos (< 30s)
2. **Isolamento**: Cada check deve ser independente
3. **Configuração**: Use configurações externalizadas
4. **Logs**: Implemente logs estruturados
5. **Tags**: Organize checks por categoria
6. **Timeouts**: Configure timeouts apropriados

## 🔗 Recursos Adicionais

- [Health checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Microsoft.Extensions.Diagnostics.HealthChecks](https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics.HealthChecks/)
- [Health Check Libraries](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
