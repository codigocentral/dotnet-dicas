# Dica 65: OpenTelemetry em .NET

## 📋 Sobre a Dica

Esta dica demonstra como implementar **observabilidade completa** em aplicações .NET usando **OpenTelemetry**, incluindo traces distribuídos, métricas customizadas e logs correlacionados.

## 🎯 Conceitos Demonstrados

### 1. Traces (Rastreamento Distribuído)
- **Activity Sources customizados** para instrumentação manual
- **Instrumentação automática** do ASP.NET Core e HttpClient
- **Spans hierárquicos** com parent-child relationships
- **Tags e atributos** para enriquecimento contextual
- **Sampling** para controle de performance

### 2. Métricas (Metrics)
- **Counters**: Contadores monotônicos (requests, errors, operations)
- **Histograms**: Distribuições de valores (duração, tamanhos)
- **Gauges**: Valores atuais (conexões ativas, uso de memória)
- **UpDownCounters**: Valores que sobem/descem (fila de itens)
- **Métricas de sistema** automáticas (ASP.NET Core, Kestrel)

### 3. Logs Correlacionados
- **Integração com ILogger** nativo do .NET
- **Correlação automática** com traces (TraceId/SpanId)
- **Logs estruturados** com atributos contextuais
- **Resource attributes** para identificação de serviço

### 4. Exportadores
- **Console Exporter** para desenvolvimento e debugging
- **Prometheus Exporter** para métricas (/metrics endpoint)
- Suporte para **Jaeger** (comentado para simplicidade)
- Configuração para múltiplos exportadores

## 🏗️ Estrutura do Projeto

```
Dica65-OpenTelemetry/
├── Telemetry/
│   └── ApplicationTelemetry.cs     # Sistema central de telemetria
├── Services/
│   └── TelemetryServices.cs        # Serviços com instrumentação
├── Controllers/
│   └── TelemetryControllers.cs     # APIs com telemetria automática
├── Program.cs                      # Configuração OpenTelemetry
├── appsettings.json               # Configurações
└── README.md
```

## 🔧 Sistema de Telemetria

### ApplicationTelemetry.cs
Sistema central que define todos os instrumentos de telemetria:

```csharp
public static class ApplicationTelemetry
{
    // Activity Source para traces
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);
    
    // Meter para métricas
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);
    
    // Contadores
    public static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>("http_requests_total");
    public static readonly Counter<long> ErrorCounter = Meter.CreateCounter<long>("errors_total");
    
    // Histogramas
    public static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>("http_request_duration_ms");
    public static readonly Histogram<double> DatabaseQueryDuration = Meter.CreateHistogram<double>("database_query_duration_ms");
    
    // Gauges Observáveis
    public static readonly ObservableGauge<long> ActiveConnections = Meter.CreateObservableGauge<long>("active_connections");
    public static readonly ObservableGauge<double> MemoryUsage = Meter.CreateObservableGauge<double>("memory_usage_mb");
}
```

### Instrumentação Manual
```csharp
// Criando span customizado
using var activity = ApplicationTelemetry.StartActivity("Business Operation");
activity?.SetTag("operation.type", "user_registration");
activity?.SetTag("user.id", userId);

// Registrando métricas
ApplicationTelemetry.RecordHttpRequest("POST", "/api/users", 201, durationMs);
ApplicationTelemetry.RequestCounter.Add(1, new KeyValuePair<string, object?>[]
{
    new("method", "POST"),
    new("endpoint", "/api/users")
});
```

## ⚙️ Configuração

### Program.cs - Configuração Completa
```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
    {
        resourceBuilder
            .AddService(serviceName: "Dica65.OpenTelemetry", serviceVersion: "1.0.0")
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("deployment.environment", "Production"),
                new KeyValuePair<string, object>("host.name", Environment.MachineName)
            });
    })
    
    // === TRACES ===
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .AddSource(ApplicationTelemetry.ActivitySource.Name)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .SetSampler(new TraceIdRatioBasedSampler(1.0))
            .AddConsoleExporter();
    })
    
    // === METRICS ===
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .AddMeter(ApplicationTelemetry.Meter.Name)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .AddPrometheusExporter();
    });
```

### appsettings.json
```json
{
  "OpenTelemetry": {
    "ServiceName": "Dica65.OpenTelemetry",
    "ServiceVersion": "1.0.0",
    "ServiceNamespace": "dicas-csharp",
    "Sampling": {
      "Ratio": 1.0
    },
    "Exporters": {
      "Console": { "Enabled": true },
      "Prometheus": { "Enabled": true }
    }
  }
}
```

## 🚀 Como Executar

```bash
# Navegar para o diretório
cd Dicas/Dica65-OpenTelemetry

# Executar a aplicação
dotnet run
```

A aplicação iniciará em `http://localhost:5000` com os seguintes endpoints:

## 📊 Endpoints de Demonstração

### Endpoints de API
- **GET** `/health` - Health check básico
- **GET** `/demo` - Demonstração completa com múltiplas operações
- **GET** `/api/users` - Listar usuários (com simulação de DB)
- **POST** `/api/users` - Criar usuário (processo completo de negócio)
- **GET** `/api/users/{id}` - Buscar usuário específico
- **PUT** `/api/users/{id}` - Atualizar usuário
- **POST** `/api/processing/batch` - Processamento em lote
- **POST** `/api/processing/complex` - Operação complexa multi-etapas

### Endpoints de Observabilidade
- **GET** `/metrics` - Métricas Prometheus

## 🔍 Exemplos de Telemetria Gerada

### Trace Distribuído
```
Activity.TraceId: 1143fa271c9722d465db624355164c49
Activity.SpanId: 5d90ab098826a80c
Activity.DisplayName: Demo Endpoint
Activity.Kind: Internal
Activity.Duration: 00:00:00.9447760
Activity.Tags:
    service.name: Dica65.OpenTelemetry
    demo.type: comprehensive
    demo.operations_completed: 3
```

### Logs Correlacionados
```
LogRecord.TraceId: 1143fa271c9722d465db624355164c49
LogRecord.SpanId: c07343bcfd5f2b09
LogRecord.CategoryName: Dica65.OpenTelemetry.Services.BusinessService
LogRecord.Body: Usuário registrado com sucesso: ID {UserId} em {Duration}ms
LogRecord.Attributes:
    UserId: 6288
    Duration: 510
```

### Métricas Customizadas
```
Metric: business_operations_total
Labels: operation_type=user_registration, result=success
Value: 1

Metric: database_query_duration_ms
Labels: query_type=INSERT, table=users, success=true
Histogram: Sum=233ms, Count=1, P95=233ms
```

## 🎯 Cenários de Demonstração

### 1. Endpoint `/demo`
Executa um fluxo completo com:
- Busca de usuários no banco
- Processamento de lote de dados
- Registro de novo usuário com validações externas
- Geração de traces hierárquicos e métricas

### 2. Operação de Negócio Completa (`POST /api/users`)
- Validação de email em serviço externo
- Criação no banco de dados
- Envio de evento de boas-vindas
- Trace distribuído com 4+ spans

### 3. Processamento em Lote (`POST /api/processing/batch`)
- Processamento de múltiplos itens
- Simulação de falhas parciais
- Métricas de throughput e taxa de erro

### 4. Operação Multi-Etapas (`POST /api/processing/complex`)
- 3 etapas sequenciais com spans separados
- Demonstração de hierarquia de traces
- Medição de performance por etapa

## 📈 Tipos de Métricas Coletadas

### Métricas de Negócio
- `business_operations_total` - Total de operações de negócio
- `processing_items_count` - Itens processados em lotes
- `database_query_duration_ms` - Duração de queries de banco

### Métricas de Sistema
- `http_requests_total` - Total de requisições HTTP
- `errors_total` - Total de erros
- `active_connections` - Conexões ativas (gauge)
- `memory_usage_mb` - Uso de memória (gauge)
- `queue_size` - Tamanho da fila (up/down counter)

### Métricas Automáticas (ASP.NET Core)
- `http.server.request.duration` - Duração de requisições
- `kestrel.active_connections` - Conexões ativas do Kestrel
- `aspnetcore.routing.match_attempts` - Tentativas de roteamento

## 🏷️ Sistema de Tags

### Tags de Negócio
- `business.operation` - Tipo da operação de negócio
- `business.domain` - Domínio da operação
- `business.result` - Resultado da operação
- `business.user_id` - ID do usuário

### Tags Técnicas
- `http.method` / `http.status_code` - HTTP específico
- `db.system` / `db.operation` / `db.table` - Database específico
- `api.service` - Serviço externo
- `service.name` / `service.version` - Identificação do serviço

### Tags de Sistema
- `deployment.environment` - Ambiente de deployment
- `host.name` - Nome do host
- `process.id` - ID do processo

## 🔧 Configurações Avançadas

### Sampling
```csharp
.SetSampler(new TraceIdRatioBasedSampler(0.5)) // 50% dos traces
```

### Enrichment Customizado
```csharp
.AddAspNetCoreInstrumentation(options =>
{
    options.EnrichWithHttpRequest = (activity, request) =>
    {
        activity.SetTag("custom.user_id", request.Headers["X-User-Id"]);
    };
})
```

### Resource Attributes
```csharp
.ConfigureResource(resourceBuilder =>
{
    resourceBuilder.AddAttributes(new[]
    {
        new KeyValuePair<string, object>("service.namespace", "dicas-csharp"),
        new KeyValuePair<string, object>("service.instance.id", Guid.NewGuid())
    });
})
```

## 🎓 Conceitos Importantes

### Activity vs Span
- **Activity** é o conceito .NET para spans do OpenTelemetry
- Cada Activity representa uma operação com início/fim
- Activities podem ter parent-child relationships

### Instrumentação Automática vs Manual
- **Automática**: ASP.NET Core, HttpClient, Entity Framework
- **Manual**: Operações de negócio, integrações customizadas

### Sampling e Performance
- **100% sampling** para desenvolvimento
- **Sampling baseado em ratio** para produção
- **Head-based sampling** vs **Tail-based sampling**

### Correlação de Logs
- TraceId/SpanId automaticamente adicionados aos logs
- Permite rastrear requests através de múltiplos serviços
- Essencial para debugging distribuído

## 💡 Melhores Práticas

### 1. **Naming Conventions**
- Nomes descritivos para activities (`Business Operation`, `DB Query`)
- Consistência em prefixos de métricas
- Tags padronizadas

### 2. **Performance**
- Use sampling em produção
- Evite tags com alta cardinalidade
- Configure timeouts apropriados

### 3. **Segurança**
- Evite dados sensíveis em tags/logs
- Configure sampling para reduzir dados
- Use filtros para excluir endpoints sensíveis

### 4. **Monitoramento**
- Configure alertas em métricas de erro
- Monitore latência P95/P99
- Acompanhe taxa de sampling

## 🔗 Integrações Externas

### Prometheus + Grafana
```bash
# Acessar métricas
curl http://localhost:5000/metrics

# Configurar Prometheus para scraping
# Target: localhost:5000/metrics
```

### Jaeger (Traces)
```csharp
// Descomentar no Program.cs
.AddJaegerExporter(options =>
{
    options.Endpoint = new Uri("http://localhost:14268/api/traces");
})
```

### APM Tools
- **Application Insights** (Azure)
- **Datadog APM**
- **New Relic**
- **Elastic APM**

## 🔗 Recursos Adicionais

- [OpenTelemetry .NET Documentation](https://github.com/open-telemetry/opentelemetry-dotnet)
- [OpenTelemetry Specification](https://opentelemetry.io/docs/specs/otel/)
- [ASP.NET Core OpenTelemetry](https://learn.microsoft.com/en-us/aspnet/core/log-mon/telemetry-opentelemetry)
- [Prometheus Metrics](https://prometheus.io/docs/concepts/metric_types/)
