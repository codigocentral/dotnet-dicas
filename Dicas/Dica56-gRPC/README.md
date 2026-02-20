# 🚀 Dica 56: gRPC com ASP.NET Core

## 📖 Sobre gRPC

gRPC (Google Remote Procedure Call) é um framework de RPC moderno, aberto e de alto desempenho que pode ser executado em qualquer ambiente. Ele utiliza HTTP/2 para transporte, Protocol Buffers como linguagem de descrição de interface e fornece recursos como autenticação, balanceamento de carga, etc.

## 🎯 Funcionalidades Demonstradas

### 1. **Tipos de Comunicação gRPC**
- **Unary RPC**: Simples request/response
- **Server Streaming**: Servidor envia múltiplas respostas
- **Client Streaming**: Cliente envia múltiplos requests
- **Bidirectional Streaming**: Ambos enviam múltiplas mensagens

### 2. **Operações CRUD**
- Criar, ler, atualizar e deletar produtos
- Validação e tratamento de erros
- Busca e filtragem

### 3. **Streaming Avançado**
- Stream de dados em tempo real
- Upload de arquivos em chunks
- Chat bidirectional
- Monitoramento de métricas

## 🛠️ Como Executar

```bash
# Navegar para o diretório
cd Dicas/Dica56-gRPC

# Executar o projeto
dotnet run

# O servidor estará disponível em:
# http://localhost:5056
```

## 📡 Testando os Serviços

### Usando grpcurl

```bash
# Instalar grpcurl
brew install grpcurl  # macOS
# ou baixar de: https://github.com/fullstorydev/grpcurl

# Listar serviços disponíveis
grpcurl -plaintext localhost:5056 list

# Listar métodos de um serviço
grpcurl -plaintext localhost:5056 list greet.Greeter

# Chamada unária simples
grpcurl -plaintext -d '{"name": "Desenvolvedor"}' \
  localhost:5056 greet.Greeter/SayHello

# Obter todos os produtos
grpcurl -plaintext -d '{}' \
  localhost:5056 products.ProductService/GetProducts

# Criar um novo produto
grpcurl -plaintext -d '{
  "name": "Novo Produto",
  "description": "Descrição do produto",
  "price": 99.99,
  "category": "Eletrônicos",
  "stock_quantity": 10,
  "tags": ["novo", "teste"]
}' localhost:5056 products.ProductService/CreateProduct

# Buscar produtos
grpcurl -plaintext -d '{
  "query": "Samsung",
  "category": "Eletrônicos",
  "min_price": 1000,
  "max_price": 5000
}' localhost:5056 products.ProductService/SearchProducts
```

### Usando BloomRPC

1. Baixe o BloomRPC: https://github.com/bloomrpc/bloomrpc
2. Conecte em `localhost:5056`
3. Importe os arquivos `.proto` da pasta `Protos/`
4. Teste os métodos disponíveis

## 📊 Exemplos de Streaming

### Server Streaming - Dados em Tempo Real

```bash
# Stream de temperatura por 30 segundos
grpcurl -plaintext -d '{
  "data_type": "temperature",
  "interval_seconds": 2,
  "duration_seconds": 30
}' localhost:5056 streaming.StreamingService/StreamData
```

### Monitoramento de Métricas

```bash
# Monitorar múltiplas métricas
grpcurl -plaintext -d '{
  "metric_names": ["requests_per_second", "error_rate", "response_time"],
  "interval_seconds": 5
}' localhost:5056 streaming.StreamingService/MonitorMetrics
```

## 🏗️ Estrutura do Projeto

```
Dica56-gRPC/
├── Protos/                 # Definições Protocol Buffers
│   ├── greet.proto         # Serviço de saudação
│   ├── products.proto      # Serviço de produtos
│   ├── orders.proto        # Serviço de pedidos
│   └── streaming.proto     # Serviços de streaming
├── Services/               # Implementações dos serviços gRPC
│   ├── GreeterService.cs   # Demonstra tipos de streaming
│   ├── ProductGrpcService.cs # CRUD de produtos
│   └── StreamingGrpcService.cs # Funcionalidades avançadas
├── Models/                 # Modelos e repositórios
│   └── ProductRepository.cs # Repositório em memória
├── Program.cs              # Configuração da aplicação
└── README.md              # Esta documentação
```

## 💡 Conceitos Importantes

### Protocol Buffers

```protobuf
syntax = "proto3";

message Product {
  int32 id = 1;
  string name = 2;
  double price = 3;
  repeated string tags = 4;
}

service ProductService {
  rpc GetProduct (GetProductRequest) returns (Product);
  rpc GetProducts (Empty) returns (ProductList);
}
```

### Vantagens do gRPC

1. **Performance**: Binary protocol, HTTP/2, compressão
2. **Type Safety**: Contratos strongly-typed
3. **Streaming**: Suporte nativo para streaming
4. **Multiplexing**: Múltiplas chamadas simultâneas
5. **Interoperabilidade**: Funciona entre diferentes linguagens

### Interceptors e Middleware

```csharp
// Configuração no Program.cs
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 4 * 1024 * 1024;
});
```

## 🔧 Configurações Avançadas

### Compressão

```csharp
// Cliente
var channel = GrpcChannel.ForAddress("https://localhost:5056", new GrpcChannelOptions
{
    CompressionProviders = new[] { new GzipCompressionProvider() }
});
```

### Autenticação

```csharp
// JWT Bearer Token
var headers = new Metadata
{
    { "Authorization", "Bearer " + token }
};

await client.GetProductAsync(request, headers);
```

### Interceptors Personalizados

```csharp
public class LoggingInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        // Log antes da chamada
        var response = await continuation(request, context);
        // Log depois da chamada
        return response;
    }
}
```

## 📈 Performance e Benchmarks

### Comparação com REST API

| Aspecto | gRPC | REST |
|---------|------|------|
| Protocol | Binary (protobuf) | Text (JSON) |
| Transport | HTTP/2 | HTTP/1.1 |
| Payload Size | ~30% menor | Baseline |
| Latência | ~20% menor | Baseline |
| Throughput | ~2x maior | Baseline |

### Medições Típicas

- **Serialização**: 3-10x mais rápida que JSON
- **Tamanho**: 20-50% menor que JSON
- **Latência**: 10-30% menor que REST
- **CPU**: 20-40% menos uso

## 🚀 Casos de Uso Ideais

1. **Microserviços**: Comunicação interna entre serviços
2. **APIs de Alto Volume**: Sistemas com muitas chamadas
3. **Streaming**: Dados em tempo real
4. **Mobile**: Menos bateria, menos dados
5. **IoT**: Protocolo eficiente para dispositivos

## 📚 Recursos Adicionais

- [Documentação oficial gRPC](https://grpc.io/)
- [gRPC com .NET](https://docs.microsoft.com/aspnet/core/grpc/)
- [Protocol Buffers](https://developers.google.com/protocol-buffers)
- [grpcurl - CLI tool](https://github.com/fullstorydev/grpcurl)
- [BloomRPC - GUI client](https://github.com/bloomrpc/bloomrpc)

## 🎓 Próximos Passos

1. Implementar autenticação e autorização
2. Adicionar interceptors personalizados
3. Configurar load balancing
4. Implementar circuit breaker
5. Adicionar monitoring e métricas
6. Criar cliente gRPC-Web para browser
7. Implementar versionamento de APIs
