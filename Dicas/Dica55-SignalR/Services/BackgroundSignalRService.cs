using Microsoft.AspNetCore.SignalR;
using Dica55_SignalR.Hubs;
using Dica55_SignalR.Models;
using Dica55_SignalR.Services;

namespace Dica55_SignalR.Services
{
    /// <summary>
    /// Serviço em background que executa demonstrações automáticas do SignalR
    /// </summary>
    public class BackgroundSignalRService : BackgroundService
    {
        private readonly ILogger<BackgroundSignalRService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random;

        public BackgroundSignalRService(
            ILogger<BackgroundSignalRService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _random = new Random();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Serviço em background do SignalR iniciado");

            // Aguardar alguns segundos para a aplicação inicializar completamente
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    
                    // Executar diferentes tipos de demonstrações em intervalos
                    await RunChatDemo(scope, stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

                    await RunNotificationDemo(scope, stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

                    await RunMonitoringDemo(scope, stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);

                    await RunCollaborationDemo(scope, stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(4), stoppingToken);

                    await RunGameDemo(scope, stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro no serviço em background do SignalR");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("⏹️ Serviço em background do SignalR finalizado");
        }

        private async Task RunChatDemo(IServiceScope scope, CancellationToken cancellationToken)
        {
            try
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
                var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

                _logger.LogInformation("🤖 Executando demonstração de chat automática...");

                var botMessages = new[]
                {
                    new { user = "DemoBot", message = "🎯 Demonstração automática do SignalR em andamento!" },
                    new { user = "AssistentAI", message = "💬 Este é um exemplo de chat em tempo real." },
                    new { user = "MonitorBot", message = "📊 Tudo funcionando perfeitamente no sistema!" },
                    new { user = "NotifyBot", message = "🔔 Demonstração de múltiplos usuários conversando." }
                };

                foreach (var msg in botMessages)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var chatMessage = new ChatMessage
                    {
                        Id = Guid.NewGuid().ToString(),
                        User = msg.user,
                        Message = msg.message,
                        Room = "demo",
                        Timestamp = DateTime.UtcNow,
                        MessageType = MessageType.Text
                    };

                    // Simular salvamento da mensagem (log apenas)
                    _logger.LogInformation("💾 Mensagem salva: {User} - {Message}", chatMessage.User, chatMessage.Message);
                    
                    await hubContext.Clients.Group(SignalRGroups.ChatRoom("demo"))
                                           .SendAsync(SignalREvents.ReceiveMessage, chatMessage, cancellationToken);

                    _logger.LogInformation("💬 Mensagem demo enviada: {User} - {Message}", msg.user, msg.message);
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na demonstração de chat");
            }
        }

        private async Task RunNotificationDemo(IServiceScope scope, CancellationToken cancellationToken)
        {
            try
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                _logger.LogInformation("🔔 Executando demonstração de notificações...");

                var notifications = new[]
                {
                    new Notification 
                    { 
                        Title = "Sistema Atualizado", 
                        Message = "Nova versão do SignalR demo disponível",
                        Type = NotificationType.Info,
                        UserId = "all"
                    },
                    new Notification 
                    { 
                        Title = "Backup Concluído", 
                        Message = "Backup automático realizado com sucesso",
                        Type = NotificationType.Success,
                        UserId = "all"
                    },
                    new Notification 
                    { 
                        Title = "Conexões Ativas", 
                        Message = $"Atualmente {_random.Next(50, 200)} usuários conectados",
                        Type = NotificationType.Info,
                        UserId = "all"
                    }
                };

                foreach (var notification in notifications)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    notification.Id = Guid.NewGuid().ToString();
                    notification.CreatedAt = DateTime.UtcNow;

                    await hubContext.Clients.Group(SignalRGroups.Notifications)
                                           .SendAsync(SignalREvents.ReceiveNotification, notification, cancellationToken);

                    _logger.LogInformation("🔔 Notificação demo enviada: {Title}", notification.Title);
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na demonstração de notificações");
            }
        }

        private async Task RunMonitoringDemo(IServiceScope scope, CancellationToken cancellationToken)
        {
            try
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<MonitoringHub>>();

                _logger.LogInformation("📊 Executando demonstração de monitoramento...");

                // Enviar várias métricas simuladas
                for (int i = 0; i < 10; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var metric = new LiveMetric
                    {
                        Name = $"Demo Metric {i + 1}",
                        Value = Math.Round(_random.NextDouble() * 100, 2),
                        Unit = i % 2 == 0 ? "%" : "MB",
                        Category = "Demo",
                        Timestamp = DateTime.UtcNow
                    };

                    await hubContext.Clients.Group(SignalRGroups.Monitoring)
                                           .SendAsync(SignalREvents.MetricUpdate, metric, cancellationToken);

                    _logger.LogInformation("📊 Métrica demo enviada: {Name} = {Value} {Unit}", 
                        metric.Name, metric.Value, metric.Unit);
                    
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }

                // Enviar dados de performance
                var performance = new PerformanceData
                {
                    CpuUsage = Math.Round(_random.NextDouble() * 100, 2),
                    MemoryUsage = Math.Round(_random.NextDouble() * 16, 2),
                    ActiveConnections = _random.Next(50, 200),
                    RequestsPerSecond = _random.Next(100, 1000),
                    ResponseTime = Math.Round(_random.NextDouble() * 500, 2),
                    ErrorRate = _random.Next(0, 5),
                    Timestamp = DateTime.UtcNow
                };

                await hubContext.Clients.Group(SignalRGroups.Monitoring)
                                       .SendAsync(SignalREvents.PerformanceUpdate, performance, cancellationToken);

                _logger.LogInformation("🎯 Dados de performance demo enviados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na demonstração de monitoramento");
            }
        }

        private async Task RunCollaborationDemo(IServiceScope scope, CancellationToken cancellationToken)
        {
            try
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<CollaborationHub>>();
                var documentId = $"demo-doc-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

                _logger.LogInformation("📝 Executando demonstração de colaboração no documento {DocumentId}...", documentId);

                var edits = new[]
                {
                    new DocumentEdit 
                    { 
                        UserId = "DemoUser1", 
                        Operation = EditOperation.Insert, 
                        Position = 0, 
                        Content = "# Documento Demo\n",
                        Timestamp = DateTime.UtcNow
                    },
                    new DocumentEdit 
                    { 
                        UserId = "DemoUser2", 
                        Operation = EditOperation.Insert, 
                        Position = 18, 
                        Content = "\nEste é um exemplo de edição colaborativa em tempo real.\n",
                        Timestamp = DateTime.UtcNow
                    },
                    new DocumentEdit 
                    { 
                        UserId = "DemoUser3", 
                        Operation = EditOperation.Insert, 
                        Position = 80, 
                        Content = "\n## Recursos Demonstrados\n- Edição simultânea\n- Sincronização em tempo real\n- Controle de versão\n",
                        Timestamp = DateTime.UtcNow
                    }
                };

                foreach (var edit in edits)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    edit.Id = Guid.NewGuid().ToString();

                    await hubContext.Clients.Group(SignalRGroups.Document(documentId))
                                           .SendAsync(SignalREvents.DocumentEdited, edit, cancellationToken);

                    _logger.LogInformation("📝 Edição demo aplicada: {UserId} - {Operation} na posição {Position}", 
                        edit.UserId, edit.Operation, edit.Position);
                    
                    await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na demonstração de colaboração");
            }
        }

        private async Task RunGameDemo(IServiceScope scope, CancellationToken cancellationToken)
        {
            try
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
                var gameId = $"demo-game-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

                _logger.LogInformation("🎮 Executando demonstração de jogo {GameId}...", gameId);

                // Simular jogadores entrando
                var players = new[]
                {
                    new GamePlayer { Id = "DemoPlayer1", Name = "Jogador Demo 1", Status = PlayerStatus.Ready },
                    new GamePlayer { Id = "DemoPlayer2", Name = "Jogador Demo 2", Status = PlayerStatus.Ready },
                    new GamePlayer { Id = "DemoPlayer3", Name = "Jogador Demo 3", Status = PlayerStatus.Ready }
                };

                foreach (var player in players)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    player.JoinedAt = DateTime.UtcNow;
                    
                    await hubContext.Clients.Group(SignalRGroups.Game(gameId))
                                           .SendAsync(SignalREvents.PlayerJoined, player, cancellationToken);

                    _logger.LogInformation("👤 Jogador demo entrou: {PlayerName}", player.Name);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }

                // Iniciar jogo
                await hubContext.Clients.Group(SignalRGroups.Game(gameId))
                                       .SendAsync(SignalREvents.GameStarted, new 
                                       { 
                                           GameId = gameId, 
                                           StartTime = DateTime.UtcNow,
                                           Players = players.Select(p => p.Name)
                                       }, cancellationToken);

                _logger.LogInformation("🚀 Jogo demo iniciado");
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                // Simular algumas jogadas
                for (int round = 1; round <= 3; round++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    foreach (var player in players)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        var move = new Dictionary<string, object>
                        {
                            ["playerId"] = player.Id,
                            ["move"] = $"move_{_random.Next(1, 10)}",
                            ["round"] = round,
                            ["timestamp"] = DateTime.UtcNow
                        };

                        await hubContext.Clients.Group(SignalRGroups.Game(gameId))
                                               .SendAsync(SignalREvents.PlayerMove, move, cancellationToken);

                        _logger.LogInformation("🎯 Jogada demo: {PlayerName} - Round {Round}", player.Name, round);
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    }

                    // Atualizar placar
                    var scores = players.Select(p => new { PlayerId = p.Id, Score = _random.Next(0, 100) }).ToList();
                    await hubContext.Clients.Group(SignalRGroups.Game(gameId))
                                           .SendAsync(SignalREvents.ScoreUpdate, scores, cancellationToken);

                    _logger.LogInformation("🏆 Placar demo atualizado - Round {Round}", round);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }

                // Finalizar jogo
                var winner = players[_random.Next(players.Length)];
                await hubContext.Clients.Group(SignalRGroups.Game(gameId))
                                       .SendAsync(SignalREvents.GameEnded, new 
                                       { 
                                           GameId = gameId, 
                                           EndTime = DateTime.UtcNow,
                                           Winner = winner.Name,
                                           FinalScores = players.Select(p => new { p.Name, Score = _random.Next(50, 100) })
                                       }, cancellationToken);

                _logger.LogInformation("🏁 Jogo demo finalizado. Vencedor: {WinnerName}", winner.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na demonstração de jogo");
            }
        }
    }
}
