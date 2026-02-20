# 🏗️ Dica 90: Microservices Patterns (.NET 9)

## 📋 Sobre

Esta dica demonstra os principais padrões utilizados em arquiteturas de microservices em .NET 9, incluindo Service Discovery, Circuit Breaker, Retry Pattern, Bulkhead, Health Checks e Event Sourcing.

## 🎯 Conceitos Demonstrados

### 1. 🔍 Service Discovery Pattern
- Registro automático de serviços
- Descoberta dinâmica de endpoints
- Health checks automáticos dos serviços
- Balanceamento de carga simples

### 2. ⚡ Circuit Breaker Pattern
- Proteção contra falhas em cascata
- Estados: Closed, Open, Half-Open
- Recuperação automática
- Configuração de thresholds

### 3. 🔄 Retry Pattern
- Políticas de retry configuráveis
- Backoff strategies: Fixed, Linear, Exponential
- Tratamento de falhas transientes
- Limites de tentativas

### 4. 🛡️ Bulkhead Pattern
- Isolamento de recursos críticos
- Pools de threads separados
- Prevenção de resource starvation
- Controle de concorrência

### 5. 💓 Health Check Pattern
- Monitoramento contínuo de saúde
- Diferentes tipos de verificações
- Agregação de status
- Métricas de performance

### 6. 📚 Event Sourcing
- Armazenamento de eventos
- Reconstrução de estado
- Auditoria completa
- Replay de eventos

## 🚀 Como Executar

```bash
dotnet run
```

## 📊 Saída Esperada

```
🏗️ Dica 90: Microservices Patterns (.NET 9)
============================================

1. 🔍 Service Discovery Pattern:
─────────────────────────────────
🔍 Simulando Service Discovery:
🔍 Serviço registrado: user-service -> http://localhost:5001
🔍 Serviço registrado: order-service -> http://localhost:5002
🔍 Serviço registrado: payment-service -> http://localhost:5003
✅ User Service descoberto: http://localhost:5001 (v1.0)
✅ Order Service descoberto: http://localhost:5002 (v1.2)
✅ Total de serviços registrados: 3

2. ⚡ Circuit Breaker Pattern:
─────────────────────────────────
⚡ Demonstrando Circuit Breaker:
   Tentativa 1...
❌ Falha simulada na tentativa 1
...

3. 🔄 Retry Pattern:
────────────────────
🔄 Demonstrando Retry Pattern:
🔄 Teste 1 - Operação instável:
   Tentativa 1...
   Aguardando 1000ms antes da próxima tentativa...
...

4. 🛡️ Bulkhead Pattern:
───────────────────────
🛡️ Demonstrando Bulkhead Pattern:
🛡️ Pool 'critical' criado com 2 slots
🛡️ Pool 'normal' criado com 1 slots
...

5. 💓 Health Check Pattern:
───────────────────────────
💓 Demonstrando Health Checks:
💓 Health check registrado: database
💓 Health check registrado: cache
💓 Health check registrado: external-api
📊 Status geral: Unhealthy
...

6. 📚 Event Sourcing:
─────────────────────
📚 Demonstrando Event Sourcing:
📚 Evento salvo: OrderCreated
📚 Evento salvo: ItemAdded
...
```

## 🔧 Funcionalidades

### Service Discovery
- ✅ Registro dinâmico de serviços
- ✅ Descoberta por nome
- ✅ Versionamento de APIs
- ✅ Health monitoring automático

### Circuit Breaker
- ✅ Detecção automática de falhas
- ✅ Transição de estados
- ✅ Recuperação inteligente
- ✅ Configuração flexível

### Retry Policy
- ✅ Múltiplas estratégias de backoff
- ✅ Configuração de limites
- ✅ Tratamento de exceções específicas
- ✅ Métricas de tentativas

### Bulkhead Pattern
- ✅ Isolamento de recursos
- ✅ Controle de concorrência
- ✅ Priorização de workloads
- ✅ Prevenção de cascading failures

### Health Checks
- ✅ Verificações assíncronos
- ✅ Agregação de status
- ✅ Métricas de latência
- ✅ Descrições detalhadas

### Event Sourcing
- ✅ Persistência de eventos
- ✅ Reconstrução de estado
- ✅ Histórico completo
- ✅ Agregação de dados

## 🎓 Conceitos Aprendidos

- **Resilience Patterns**: Como implementar padrões de resiliência
- **Service Communication**: Comunicação entre microservices
- **Monitoring**: Monitoramento e observabilidade
- **Event-Driven Architecture**: Arquitetura orientada a eventos
- **Resource Management**: Gerenciamento de recursos compartilhados
- **Fault Tolerance**: Tolerância a falhas

## 📚 Referências

- [Microsoft Microservices Guide](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Circuit Breaker Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
- [Health Check Pattern](https://microservices.io/patterns/observability/health-check-api.html)
- [Event Sourcing](https://microservices.io/patterns/data/event-sourcing.html)
