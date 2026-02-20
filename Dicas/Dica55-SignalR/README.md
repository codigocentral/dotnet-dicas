# 🎯 Dica 55: SignalR - Comunicação em Tempo Real

> **Demonstração Completa**: Como implementar comunicação bidirecional em tempo real com ASP.NET Core SignalR

## 📋 Visão Geral

O SignalR é uma biblioteca que permite comunicação em tempo real entre servidor e cliente, suportando:
- **WebSockets** (primeira opção)
- **Server-Sent Events** (fallback)
- **Long Polling** (fallback final)

Esta implementação demonstra **5 cenários práticos** de uso do SignalR.

## 🎯 Cenários Implementados

### 1. 💬 Chat em Tempo Real (`ChatHub`)
```csharp
// Funcionalidades:
• Salas de chat públicas e privadas
• Mensagens privadas entre usuários
• Indicadores de "digitando"
• Status de usuário (online, ausente, ocupado)
• Histórico de mensagens
• Moderação e administração
```

### 2. 🔔 Notificações Push (`NotificationHub`)
```csharp
// Funcionalidades:
• Notificações personalizadas
• Broadcast para todos os usuários
• Notificações direcionadas
• Diferentes tipos (info, sucesso, aviso, erro)
• Persistência de notificações
```

### 3. 📊 Monitoramento em Tempo Real (`MonitoringHub`)
```csharp
// Funcionalidades:
• Métricas de sistema (CPU, memória, rede)
• Dados de performance em tempo real
• Alertas automáticos
• Dashboards interativos
• Histórico de métricas
```

### 4. 📝 Colaboração em Documentos (`CollaborationHub`)
```csharp
// Funcionalidades:
• Edição simultânea de documentos
• Sincronização de mudanças
• Controle de versão
• Indicadores de cursor
• Comentários colaborativos
```

### 5. 🎮 Jogos Multiplayer (`GameHub`)
```csharp
// Funcionalidades:
• Salas de jogo
• Sincronização de estado
• Sistema de pontuação
• Chat durante jogo
• Diferentes tipos de jogos
```

## 🚀 Como Executar

### Pré-requisitos
- .NET 9.0
- Visual Studio Code ou Visual Studio

### Execução
```bash
# Navegar para o diretório
cd Dicas/Dica55-SignalR

# Restaurar dependências
dotnet restore

# Executar aplicação
dotnet run

# Acessar aplicação
# http://localhost:5000
```

## 🔧 Configuração do SignalR

### 1. **Configuração Básica**
```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(20);
    options.HandshakeTimeout = TimeSpan.FromSeconds(5);
})
.AddMessagePackProtocol(); // Protocolo mais eficiente que JSON
```

### 2. **CORS para Frontend**
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### 3. **Mapeamento de Hubs**
```csharp
app.MapHub<ChatHub>("/hub/chat");
app.MapHub<NotificationHub>("/hub/notifications");
app.MapHub<MonitoringHub>("/hub/monitoring");
app.MapHub<CollaborationHub>("/hub/collaboration");
app.MapHub<GameHub>("/hub/games");
```

## 💡 Padrões Implementados

### 1. **Gerenciamento de Grupos**
```csharp
// Constantes para grupos
public static class SignalRGroups
{
    public static string ChatRoom(string roomId) => $"chat_room_{roomId}";
    public static string User(string userId) => $"user_{userId}";
    public static string Game(string gameId) => $"game_{gameId}";
    public static string Document(string docId) => $"document_{docId}";
    public const string Monitoring = "monitoring";
    public const string Notifications = "notifications";
}
```

### 2. **Eventos Padronizados**
```csharp
public static class SignalREvents
{
    // Chat
    public const string ReceiveMessage = "ReceiveMessage";
    public const string UserJoined = "UserJoined";
    public const string UserLeft = "UserLeft";
    public const string TypingIndicator = "TypingIndicator";
    
    // Notificações
    public const string ReceiveNotification = "ReceiveNotification";
    public const string BroadcastMessage = "BroadcastMessage";
    
    // Monitoramento
    public const string MetricUpdate = "MetricUpdate";
    public const string PerformanceUpdate = "PerformanceUpdate";
    
    // Colaboração
    public const string DocumentEdited = "DocumentEdited";
    public const string CursorMoved = "CursorMoved";
    
    // Jogos
    public const string PlayerJoined = "PlayerJoined";
    public const string PlayerMove = "PlayerMove";
    public const string GameStarted = "GameStarted";
    public const string GameEnded = "GameEnded";
    public const string ScoreUpdate = "ScoreUpdate";
}
```

### 3. **Arquitetura de Serviços**
```csharp
// Injeção de dependência para lógica de negócio
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddScoped<ICollaborationService, CollaborationService>();
builder.Services.AddScoped<IGameService, GameService>();
```

## 🔍 Endpoints de Demonstração

### Chat Simulation
```http
GET /demo/chat-simulation
```
Simula conversa automatizada com múltiplos bots.

### Send Notification
```http
POST /demo/send-notification
Content-Type: application/json

{
  "title": "Nova Mensagem",
  "message": "Você tem uma nova mensagem!",
  "type": "Info",
  "userId": "user123"
}
```

### Broadcast Message
```http
POST /demo/broadcast
Content-Type: application/json

{
  "title": "Manutenção Programada",
  "message": "Sistema será atualizado às 23h"
}
```

### Monitoring Data
```http
GET /demo/monitoring-data
```
Envia dados simulados de monitoramento.

### Collaboration Demo
```http
POST /demo/collaboration?documentId=doc123
```
Simula edição colaborativa em documento.

### Game Simulation
```http
POST /demo/game-simulation?gameId=game456
```
Simula jogo multiplayer completo.

## 🌐 Clientes de Teste

### Cliente de Chat Básico
```http
GET /client/chat
```
Interface web simples para testar chat em tempo real.

### Exemplo de Conexão JavaScript
```javascript
// Conectar ao hub de chat
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub/chat")
    .withAutomaticReconnect()
    .build();

// Ouvir mensagens
connection.on("ReceiveMessage", (message) => {
    console.log(`${message.user}: ${message.message}`);
});

// Conectar
await connection.start();

// Entrar em sala
await connection.invoke("JoinRoom", { roomId: "general" });

// Enviar mensagem
await connection.invoke("SendMessage", {
    message: "Olá pessoal!",
    room: "general"
});
```

## 📊 Recursos Avançados

### 1. **Autenticação e Autorização**
```csharp
[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var userName = Context.User?.Identity?.Name;
        // Lógica de autenticação
    }
}
```

### 2. **Reconexão Automática**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub/chat")
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .build();
```

### 3. **Protocolo MessagePack**
```csharp
// Servidor
.AddMessagePackProtocol()

// Cliente JavaScript
.withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
```

### 4. **Grupos Dinâmicos**
```csharp
// Entrar em grupo
await Groups.AddToGroupAsync(Context.ConnectionId, "VIP_Users");

// Sair de grupo
await Groups.RemoveFromGroupAsync(Context.ConnectionId, "VIP_Users");

// Enviar para grupo
await Clients.Group("VIP_Users").SendAsync("SpecialMessage", data);
```

### 5. **Streaming**
```csharp
// Hub método para streaming
public async IAsyncEnumerable<LiveMetric> StreamMetrics(
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        yield return GetCurrentMetric();
        await Task.Delay(1000, cancellationToken);
    }
}

// Cliente
connection.stream("StreamMetrics").subscribe({
    next: (metric) => console.log(metric),
    complete: () => console.log("Stream completed"),
    error: (err) => console.error(err)
});
```

## 🛠️ Troubleshooting

### Problemas Comuns

1. **Conexão Recusada**
   - Verificar CORS
   - Confirmar URL do hub
   - Checar firewall/proxy

2. **Mensagens Não Chegam**
   - Verificar grupos
   - Confirmar IDs de conexão
   - Validar eventos

3. **Performance**
   - Usar MessagePack
   - Implementar grupos eficientes
   - Otimizar frequência de updates

### Logs e Debugging
```csharp
// Logging detalhado
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// SignalR detalhado
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
```

## 📈 Monitoramento em Produção

### Métricas Importantes
- Número de conexões ativas
- Latência de mensagens
- Taxa de reconexões
- Uso de memória por conexão
- Throughput de mensagens

### Ferramentas Recomendadas
- **Application Insights** (Azure)
- **Prometheus + Grafana**
- **ELK Stack** (logs)
- **SignalR Scale-out** (Redis, Azure Service Bus)

## 🎯 Principais Aprendizados

### ✅ **Vantagens do SignalR**
- Fallback automático de transporte
- Integração nativa com ASP.NET Core
- Suporte a grupos e usuários
- Reconexão automática
- Streaming de dados

### ⚠️ **Considerações**
- Gerenciamento de estado em scale-out
- Limite de conexões simultâneas
- Consumo de memória por conexão
- Necessidade de load balancer sticky sessions

### 🔧 **Melhores Práticas**
- Usar grupos para otimizar envios
- Implementar throttling de mensagens
- Validar dados de entrada
- Monitorar performance
- Implementar retry policies

---

## 📚 Recursos Adicionais

- [Documentação Oficial SignalR](https://docs.microsoft.com/aspnet/core/signalr)
- [SignalR JavaScript Client](https://docs.microsoft.com/aspnet/core/signalr/javascript-client)
- [Scale-out SignalR](https://docs.microsoft.com/aspnet/core/signalr/scale)
- [SignalR Performance](https://docs.microsoft.com/aspnet/core/signalr/security)

**🎯 Objetivo**: Demonstrar implementação completa de comunicação em tempo real com SignalR, cobrindo múltiplos cenários e melhores práticas para aplicações modernas.
