using Microsoft.AspNetCore.SignalR;
using Dica55_SignalR.Hubs;
using Dica55_SignalR.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURAÇÃO DE SERVIÇOS
// ==========================================

// Configurar CORS
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

// Configurar SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(20);
})
.AddMessagePackProtocol();

// Registrar serviços especializados
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddScoped<ICollaborationService, CollaborationService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ISignalRDemoService, SignalRDemoService>();
builder.Services.AddScoped<IChatService, ChatService>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// ==========================================
// CONFIGURAÇÃO DO PIPELINE
// ==========================================

app.UseCors();

app.UseRouting();

// Mapear Hubs do SignalR
app.MapHub<ChatHub>("/hub/chat");
app.MapHub<MonitoringHub>("/hub/monitoring");
app.MapHub<CollaborationHub>("/hub/collaboration");
app.MapHub<GameHub>("/hub/game");

// ==========================================
// ENDPOINTS BÁSICOS
// ==========================================

app.MapGet("/", () => """
🎯 Dica 55: SignalR - Comunicação em Tempo Real

🔥 HUBS DISPONÍVEIS:
• /hub/chat              - Chat em tempo real
• /hub/monitoring        - Monitoramento de sistema em tempo real
• /hub/collaboration     - Colaboração em documentos
• /hub/game              - Jogos multiplayer

📊 DEMONSTRAÇÕES PRÁTICAS:
• GET  /demo/chat-simulation        - Simular chat automatizado
• GET  /demo/monitoring-simulation  - Simular métricas em tempo real
• GET  /demo/collaboration-test     - Teste de colaboração
• GET  /demo/game-simulation        - Simular jogo multiplayer

🌐 CLIENTES DE TESTE:
• GET /client/chat          - Cliente de chat básico
• GET /client/monitoring    - Dashboard de monitoramento
• GET /client/collaboration - Editor colaborativo
• GET /client/game          - Cliente de jogo

💡 OBJETIVO: Demonstrar comunicação bidirecional em tempo real
""")
.WithTags("Home")
.WithSummary("Página inicial com demonstrações SignalR");

// Simulação de chat
app.MapGet("/demo/chat-simulation", async (IHubContext<ChatHub> hubContext) =>
{
    var messages = new[]
    {
        new { user = "Bot1", message = "Olá pessoal! Como estão?" },
        new { user = "Bot2", message = "Tudo bem! Trabalhando com SignalR hoje." },
        new { user = "Bot3", message = "SignalR é incrível para tempo real!" }
    };

    foreach (var msg in messages)
    {
        await hubContext.Clients.All.SendAsync("ReceiveMessage", new 
        { 
            user = msg.user, 
            message = msg.message, 
            timestamp = DateTime.UtcNow 
        });
        
        await Task.Delay(2000);
    }

    return Results.Ok(new { message = "Simulação concluída", messagesCount = messages.Length });
});

// Simulação de monitoramento
app.MapGet("/demo/monitoring-simulation", async (IHubContext<MonitoringHub> hubContext, IMonitoringService monitoringService) =>
{
    var random = new Random();
    var metrics = new[]
    {
        "CPU Usage", "Memory Usage", "Disk I/O", "Network Traffic"
    };

    for (int i = 0; i < 10; i++)
    {
        foreach (var metricName in metrics)
        {
            var metric = new Dica55_SignalR.Models.LiveMetric
            {
                Name = metricName,
                Value = random.NextDouble() * 100,
                Unit = metricName.Contains("Usage") ? "%" : "MB/s",
                Timestamp = DateTime.UtcNow,
                Category = "System"
            };

            await monitoringService.UpdateMetricAsync(metric);
            await hubContext.Clients.All.SendAsync("MetricUpdated", metric);
        }
        
        await Task.Delay(1000);
    }

    return Results.Ok(new { message = "Simulação de monitoramento concluída", cycles = 10 });
});

// Simulação de jogo
app.MapGet("/demo/game-simulation", async (IHubContext<GameHub> hubContext, IGameService gameService) =>
{
    var gameId = Guid.NewGuid().ToString();
    
    // Criar jogo
    var game = await gameService.CreateGameAsync(gameId, Dica55_SignalR.Models.GameType.Quiz);
    await hubContext.Clients.All.SendAsync("GameCreated", game);

    // Adicionar jogadores simulados
    var players = new[]
    {
        new Dica55_SignalR.Models.GamePlayer { Id = "player1", Name = "Alice", Score = 0 },
        new Dica55_SignalR.Models.GamePlayer { Id = "player2", Name = "Bob", Score = 0 }
    };

    foreach (var player in players)
    {
        await gameService.AddPlayerAsync(gameId, player);
        await hubContext.Clients.All.SendAsync("PlayerJoined", new { gameId, player });
        await Task.Delay(1000);
    }

    return Results.Ok(new { message = "Jogo simulado criado", gameId, playersCount = players.Length });
});

// Cliente de teste
app.MapGet("/client/chat", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <title>Chat Cliente - SignalR</title>
    <script src="https://unpkg.com/@microsoft/signalr/dist/browser/signalr.min.js"></script>
</head>
<body>
    <h1>🎯 Chat Cliente SignalR</h1>
    <div id="status">Desconectado</div>
    <div>
        <input type="text" id="userInput" placeholder="Seu nome" />
        <button onclick="connect()">Conectar</button>
    </div>
    <div>
        <input type="text" id="messageInput" placeholder="Digite sua mensagem" />
        <button onclick="sendMessage()">Enviar</button>
    </div>
    <div id="messages"></div>

    <script>
        let connection = null;
        
        async function connect() {
            const user = document.getElementById('userInput').value;
            if (!user) return;
            
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/hub/chat")
                .build();
                
            connection.on("ReceiveMessage", (message) => {
                const messages = document.getElementById('messages');
                messages.innerHTML += `<div><strong>${message.user}:</strong> ${message.message}</div>`;
            });
            
            await connection.start();
            document.getElementById('status').innerText = 'Conectado como ' + user;
        }
        
        async function sendMessage() {
            const message = document.getElementById('messageInput').value;
            if (!message || !connection) return;
            
            await connection.invoke("SendMessage", message);
            document.getElementById('messageInput').value = '';
        }
    </script>
</body>
</html>
""", "text/html"))
.WithTags("Test Clients");

// Cliente de monitoramento
app.MapGet("/client/monitoring", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <title>Dashboard de Monitoramento - SignalR</title>
    <script src="https://unpkg.com/@microsoft/signalr/dist/browser/signalr.min.js"></script>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .metric { margin: 10px 0; padding: 10px; border: 1px solid #ccc; border-radius: 5px; }
        .value { font-size: 24px; font-weight: bold; color: #007acc; }
        #status { margin-bottom: 20px; padding: 10px; background: #f0f0f0; }
    </style>
</head>
<body>
    <h1>📊 Dashboard de Monitoramento SignalR</h1>
    <div id="status">Desconectado</div>
    <button onclick="connect()">Conectar</button>
    <button onclick="disconnect()">Desconectar</button>
    
    <div id="metrics"></div>

    <script>
        let connection = null;
        const metrics = {};
        
        async function connect() {
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/hub/monitoring")
                .build();
                
            connection.on("MetricUpdated", (metric) => {
                metrics[metric.name] = metric;
                updateDisplay();
            });
            
            connection.on("SystemStatusUpdated", (status) => {
                console.log("Status do sistema:", status);
            });
            
            await connection.start();
            document.getElementById('status').innerText = 'Conectado ao hub de monitoramento';
            document.getElementById('status').style.background = '#d4edda';
        }
        
        async function disconnect() {
            if (connection) {
                await connection.stop();
                document.getElementById('status').innerText = 'Desconectado';
                document.getElementById('status').style.background = '#f8d7da';
            }
        }
        
        function updateDisplay() {
            const container = document.getElementById('metrics');
            container.innerHTML = '';
            
            Object.values(metrics).forEach(metric => {
                const div = document.createElement('div');
                div.className = 'metric';
                div.innerHTML = `
                    <div><strong>${metric.name}</strong></div>
                    <div class="value">${metric.value.toFixed(2)} ${metric.unit}</div>
                    <div>Categoria: ${metric.category}</div>
                    <div>Atualizado: ${new Date(metric.timestamp).toLocaleTimeString()}</div>
                `;
                container.appendChild(div);
            });
        }
    </script>
</body>
</html>
""", "text/html"))
.WithTags("Test Clients");

Console.WriteLine("""

🎯 DICA 55: SIGNALR - COMUNICAÇÃO EM TEMPO REAL
═══════════════════════════════════════════════════════════════

🔥 RECURSOS IMPLEMENTADOS:
• Chat em tempo real com SignalR
• Monitoramento de sistema em tempo real
• Colaboração em documentos
• Jogos multiplayer
• WebSockets com fallback automático

⚡ COMO USAR:
1. Acesse http://localhost:5000 para ver as opções
2. Use /client/chat para testar chat básico
3. Use /client/monitoring para dashboard em tempo real
4. Execute as demos em /demo/* para ver automação

🎮 HUBS ESPECIALIZADOS:
• MonitoringHub    - Métricas de sistema
• CollaborationHub - Edição colaborativa
• GameHub          - Jogos multiplayer
• ChatHub          - Chat básico

💡 OBJETIVO: Demonstrar comunicação bidirecional em tempo real
═══════════════════════════════════════════════════════════════

""");

app.Run();
