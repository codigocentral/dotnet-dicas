using System.Collections.Concurrent;
using Dica55_SignalR.Models;

namespace Dica55_SignalR.Services
{
    // ==========================================
    // INTERFACES DOS SERVIÇOS ESPECIALIZADOS
    // ==========================================

    /// <summary>
    /// Interface para serviços de monitoramento em tempo real
    /// </summary>
    public interface IMonitoringService
    {
        Task UpdateMetricAsync(LiveMetric metric);
        Task<IEnumerable<LiveMetric>> GetMetricsAsync();
        Task<PerformanceData> GetPerformanceDataAsync();
        Task StartSystemMonitoringAsync();
        Task StopSystemMonitoringAsync();
        Task<IEnumerable<LiveMetric>> GetCurrentMetricsAsync();
        Task<object> GetSystemStatusAsync();
    }

    /// <summary>
    /// Interface para serviços de colaboração em tempo real
    /// </summary>
    public interface ICollaborationService
    {
        Task<CollaborativeDocument> GetDocumentAsync(string documentId);
        Task ApplyEditAsync(string documentId, DocumentEdit edit);
        Task AddCollaboratorAsync(string documentId, string userId);
        Task RemoveCollaboratorAsync(string documentId, string userId);
        Task<IEnumerable<string>> GetActiveCollaboratorsAsync(string documentId);
        Task RemoveUserCursorsAsync(string userId);
        Task<bool> CanAccessDocumentAsync(string documentId, string userId);
        Task<IEnumerable<object>> GetActiveCursorsAsync(string documentId);
        Task UpdateCursorAsync(string userId, string documentId, object cursorPosition);
    }

    /// <summary>
    /// Interface para serviços de jogos multiplayer
    /// </summary>
    public interface IGameService
    {
        Task<GameRoom> CreateGameAsync(string gameId, GameType gameType);
        Task<bool> AddPlayerAsync(string gameId, GamePlayer player);
        Task RemovePlayerAsync(string gameId, string playerId);
        Task<GameRoom?> GetGameAsync(string gameId);
        Task<bool> CanJoinGameAsync(string gameId, string playerId);
        Task<object> GetGameStateAsync(string gameId);
        Task HandlePlayerDisconnectionAsync(string gameId, string playerId);
        Task<Dictionary<string, object>> ProcessMoveAsync(string gameId, string userId, object moveData);
        Task<Dictionary<string, object>> ProcessQuizAnswerAsync(string gameId, string userId, string questionId, int answerIndex);
    }

    /// <summary>
    /// Interface para demonstrações avançadas de SignalR
    /// </summary>
    public interface ISignalRDemoService
    {
        Task SendNotificationAsync(string userId, string message);
        Task BroadcastMessageAsync(string message);
        Task SendToGroupAsync(string groupName, string message);
    }

    /// <summary>
    /// Interface para serviços de chat básico
    /// </summary>
    public interface IChatService
    {
        Task RegisterUserAsync(string userId, string connectionId);
        Task UnregisterUserAsync(string userId, string connectionId);
        Task<IEnumerable<string>> GetActiveUsersAsync();
    }

    // ==========================================
    // IMPLEMENTAÇÕES DOS SERVIÇOS
    // ==========================================

    /// <summary>
    /// Serviço para monitoramento de sistema em tempo real
    /// </summary>
    public class MonitoringService : IMonitoringService
    {
        private readonly ILogger<MonitoringService> _logger;
        private readonly ConcurrentDictionary<string, LiveMetric> _metrics;
        private readonly Random _random;

        public MonitoringService(ILogger<MonitoringService> logger)
        {
            _logger = logger;
            _metrics = new ConcurrentDictionary<string, LiveMetric>();
            _random = new Random();

            // Inicializar métricas de exemplo
            InitializeSampleMetrics();
        }

        public Task UpdateMetricAsync(LiveMetric metric)
        {
            _metrics.AddOrUpdate(metric.Name, metric, (key, oldValue) => metric);
            _logger.LogInformation("📊 Métrica atualizada: {MetricName} = {Value}", metric.Name, metric.Value);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<LiveMetric>> GetMetricsAsync()
        {
            return Task.FromResult<IEnumerable<LiveMetric>>(_metrics.Values);
        }

        public Task<PerformanceData> GetPerformanceDataAsync()
        {
            return Task.FromResult(new PerformanceData
            {
                CpuUsage = _random.NextDouble() * 100,
                MemoryUsage = _random.NextDouble() * 100,
                DiskUsage = _random.NextDouble() * 100,
                ActiveConnections = _random.Next(10, 100),
                Timestamp = DateTime.UtcNow
            });
        }

        public Task StartSystemMonitoringAsync()
        {
            _logger.LogInformation("🚀 Monitoramento do sistema iniciado");
            return Task.CompletedTask;
        }

        public Task StopSystemMonitoringAsync()
        {
            _logger.LogInformation("⏹️ Monitoramento do sistema parado");
            return Task.CompletedTask;
        }

        private void InitializeSampleMetrics()
        {
            var sampleMetrics = new[]
            {
                new LiveMetric 
                { 
                    Name = "CPU Usage", 
                    Value = 45.2, 
                    Unit = "%", 
                    Timestamp = DateTime.UtcNow,
                    Category = "System"
                },
                new LiveMetric 
                { 
                    Name = "Memory Usage", 
                    Value = 68.7, 
                    Unit = "%", 
                    Timestamp = DateTime.UtcNow,
                    Category = "System"
                },
                new LiveMetric 
                { 
                    Name = "Disk I/O", 
                    Value = 23.1, 
                    Unit = "MB/s", 
                    Timestamp = DateTime.UtcNow,
                    Category = "Storage"
                },
                new LiveMetric 
                { 
                    Name = "Network Traffic", 
                    Value = 156.3, 
                    Unit = "KB/s", 
                    Timestamp = DateTime.UtcNow,
                    Category = "Network"
                }
            };

            foreach (var metric in sampleMetrics)
            {
                _metrics.TryAdd(metric.Name, metric);
            }
        }

        public Task<IEnumerable<LiveMetric>> GetCurrentMetricsAsync()
        {
            return Task.FromResult<IEnumerable<LiveMetric>>(_metrics.Values);
        }

        public Task<object> GetSystemStatusAsync()
        {
            return Task.FromResult<object>(new
            {
                Status = "Healthy",
                Uptime = TimeSpan.FromHours(24).ToString(),
                ActiveConnections = _random.Next(10, 100),
                LoadAverage = _random.NextDouble() * 2,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Serviço para colaboração em documentos em tempo real
    /// </summary>
    public class CollaborationService : ICollaborationService
    {
        private readonly ILogger<CollaborationService> _logger;
        private readonly ConcurrentDictionary<string, CollaborativeDocument> _documents;
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _documentCollaborators;

        public CollaborationService(ILogger<CollaborationService> logger)
        {
            _logger = logger;
            _documents = new ConcurrentDictionary<string, CollaborativeDocument>();
            _documentCollaborators = new ConcurrentDictionary<string, ConcurrentBag<string>>();
        }

        public Task<CollaborativeDocument> GetDocumentAsync(string documentId)
        {
            var document = _documents.GetOrAdd(documentId, id => new CollaborativeDocument
            {
                Id = id,
                Title = $"Documento {id}",
                Content = $"Conteúdo inicial do documento {id}",
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Version = 1
            });

            return Task.FromResult(document);
        }

        public Task ApplyEditAsync(string documentId, DocumentEdit edit)
        {
            if (_documents.TryGetValue(documentId, out var document))
            {
                switch (edit.Operation)
                {
                    case EditOperation.Insert:
                        if (edit.Position <= document.Content.Length)
                        {
                            document.Content = document.Content.Insert(edit.Position, edit.Content);
                        }
                        break;
                    
                    case EditOperation.Delete:
                        if (edit.Position < document.Content.Length)
                        {
                            var length = Math.Min(edit.Length, document.Content.Length - edit.Position);
                            document.Content = document.Content.Remove(edit.Position, length);
                        }
                        break;
                    
                    case EditOperation.Replace:
                        if (edit.Position < document.Content.Length)
                        {
                            var length = Math.Min(edit.Length, document.Content.Length - edit.Position);
                            document.Content = document.Content.Remove(edit.Position, length);
                            document.Content = document.Content.Insert(edit.Position, edit.Content);
                        }
                        break;
                }

                document.LastModified = DateTime.UtcNow;
                document.Version++;

                _logger.LogInformation("✏️ Edição aplicada ao documento {DocumentId}: {Operation} na posição {Position}",
                    documentId, edit.Operation, edit.Position);
            }

            return Task.CompletedTask;
        }

        public Task AddCollaboratorAsync(string documentId, string userId)
        {
            var collaborators = _documentCollaborators.GetOrAdd(documentId, _ => new ConcurrentBag<string>());
            
            // Verificar se usuário já está na lista (ConcurrentBag permite duplicatas)
            if (!collaborators.Contains(userId))
            {
                collaborators.Add(userId);
                _logger.LogInformation("👥 Colaborador {UserId} adicionado ao documento {DocumentId}", userId, documentId);
            }

            return Task.CompletedTask;
        }

        public Task RemoveCollaboratorAsync(string documentId, string userId)
        {
            if (_documentCollaborators.TryGetValue(documentId, out var collaborators))
            {
                // Criar nova lista sem o usuário (ConcurrentBag não tem Remove)
                var newCollaborators = new ConcurrentBag<string>(
                    collaborators.Where(c => c != userId));
                _documentCollaborators.TryUpdate(documentId, newCollaborators, collaborators);
                
                _logger.LogInformation("👋 Colaborador {UserId} removido do documento {DocumentId}", userId, documentId);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetActiveCollaboratorsAsync(string documentId)
        {
            var collaborators = _documentCollaborators.GetValueOrDefault(documentId, new ConcurrentBag<string>());
            return Task.FromResult<IEnumerable<string>>(collaborators.Distinct());
        }

        public Task RemoveUserCursorsAsync(string userId)
        {
            _logger.LogInformation("🖱️ Cursores do usuário {UserId} removidos", userId);
            return Task.CompletedTask;
        }

        public Task<bool> CanAccessDocumentAsync(string documentId, string userId)
        {
            // Simulação de verificação de permissões
            var canAccess = !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(documentId);
            _logger.LogInformation("🔐 Verificação de acesso: Usuário {UserId} {'pode' : 'não pode'} acessar documento {DocumentId}", 
                userId, canAccess ? "pode" : "não pode", documentId);
            return Task.FromResult(canAccess);
        }

        public Task<IEnumerable<object>> GetActiveCursorsAsync(string documentId)
        {
            // Simulação de cursores ativos
            var cursors = new[]
            {
                new { UserId = "user1", Position = 10, Color = "#FF0000" },
                new { UserId = "user2", Position = 25, Color = "#00FF00" }
            };
            return Task.FromResult<IEnumerable<object>>(cursors);
        }

        public Task UpdateCursorAsync(string userId, string documentId, object cursorPosition)
        {
            _logger.LogInformation("🖱️ Cursor atualizado: Usuário {UserId} no documento {DocumentId}", userId, documentId);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Serviço para jogos multiplayer em tempo real
    /// </summary>
    public class GameService : IGameService
    {
        private readonly ILogger<GameService> _logger;
        private readonly ConcurrentDictionary<string, GameRoom> _games;
        private readonly ConcurrentDictionary<string, List<object>> _gameMoves;
        private readonly Random _random;

        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
            _games = new ConcurrentDictionary<string, GameRoom>();
            _gameMoves = new ConcurrentDictionary<string, List<object>>();
            _random = new Random();
        }

        public Task<GameRoom> CreateGameAsync(string gameId, GameType gameType)
        {
            var game = new GameRoom
            {
                Id = gameId,
                Name = $"Sala {gameId}",
                Type = gameType,
                Status = GameStatus.Waiting,
                MaxPlayers = gameType == GameType.Multiplayer ? 8 : 2,
                CreatedAt = DateTime.UtcNow,
                Players = new List<GamePlayer>()
            };

            _games.TryAdd(gameId, game);
            _gameMoves.TryAdd(gameId, new List<object>());

            _logger.LogInformation("🎮 Novo jogo criado: {GameId} ({GameType})", gameId, gameType);
            return Task.FromResult(game);
        }

        public Task<bool> AddPlayerAsync(string gameId, GamePlayer player)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                if (game.Players.Count < game.MaxPlayers && !game.Players.Any(p => p.Id == player.Id))
                {
                    game.Players.Add(player);
                    _logger.LogInformation("🎯 Jogador {PlayerId} entrou no jogo {GameId}", player.Id, gameId);
                    
                    // Iniciar jogo se houver jogadores suficientes
                    if (game.Players.Count >= 2 && game.Status == GameStatus.Waiting)
                    {
                        game.Status = GameStatus.InProgress;
                        game.StartedAt = DateTime.UtcNow;
                        _logger.LogInformation("▶️ Jogo {GameId} iniciado!", gameId);
                    }
                    
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public Task RemovePlayerAsync(string gameId, string playerId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                var player = game.Players.FirstOrDefault(p => p.Id == playerId);
                if (player != null)
                {
                    game.Players.Remove(player);
                    _logger.LogInformation("👋 Jogador {PlayerId} saiu do jogo {GameId}", playerId, gameId);

                    // Finalizar jogo se não houver jogadores suficientes
                    if (game.Players.Count < 2 && game.Status == GameStatus.InProgress)
                    {
                        game.Status = GameStatus.Ended;
                        game.EndedAt = DateTime.UtcNow;
                        _logger.LogInformation("⏹️ Jogo {GameId} finalizado por falta de jogadores", gameId);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task<GameRoom?> GetGameAsync(string gameId)
        {
            _games.TryGetValue(gameId, out var game);
            return Task.FromResult(game);
        }

        public Task<bool> CanJoinGameAsync(string gameId, string playerId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                return Task.FromResult(
                    game.Status == GameStatus.Waiting &&
                    game.Players.Count < game.MaxPlayers &&
                    !game.Players.Any(p => p.Id == playerId));
            }
            return Task.FromResult(false);
        }

        public Task<object> GetGameStateAsync(string gameId)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                var moves = _gameMoves.GetValueOrDefault(gameId, new List<object>());
                return Task.FromResult<object>(new
                {
                    Game = game,
                    MoveHistory = moves,
                    LastUpdate = DateTime.UtcNow
                });
            }
            return Task.FromResult<object>(new { Error = "Jogo não encontrado" });
        }

        public Task HandlePlayerDisconnectionAsync(string gameId, string playerId)
        {
            _logger.LogInformation("🔌 Jogador {PlayerId} desconectado do jogo {GameId}", playerId, gameId);
            return RemovePlayerAsync(gameId, playerId);
        }

        public Task<Dictionary<string, object>> ProcessMoveAsync(string gameId, string userId, object moveData)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                var moves = _gameMoves.GetValueOrDefault(gameId, new List<object>());
                moves.Add(new { UserId = userId, Move = moveData, Timestamp = DateTime.UtcNow });
                
                _logger.LogInformation("🎯 Movimento processado no jogo {GameId} pelo jogador {UserId}", gameId, userId);
                
                return Task.FromResult(new Dictionary<string, object> { 
                    ["Success"] = true, 
                    ["GameState"] = game 
                });
            }
            
            return Task.FromResult(new Dictionary<string, object> { 
                ["Success"] = false, 
                ["Error"] = "Jogo não encontrado" 
            });
        }

        public Task<Dictionary<string, object>> ProcessQuizAnswerAsync(string gameId, string userId, string questionId, int answerIndex)
        {
            if (_games.TryGetValue(gameId, out var game))
            {
                var isCorrect = _random.Next(0, 2) == 1; // Simulação de resposta correta/incorreta
                var points = isCorrect ? 10 : 0;
                
                var player = game.Players.FirstOrDefault(p => p.Id == userId);
                if (player != null)
                {
                    player.Score += points;
                }
                
                _logger.LogInformation("🧠 Resposta do quiz processada: Jogador {UserId}, Questão {QuestionId}, Resposta {AnswerIndex}, Pontos: {Points}", 
                    userId, questionId, answerIndex, points);
                
                return Task.FromResult(new Dictionary<string, object>
                { 
                    ["Success"] = true, 
                    ["IsCorrect"] = isCorrect, 
                    ["Points"] = points,
                    ["TotalScore"] = player?.Score ?? 0
                });
            }
            
            return Task.FromResult(new Dictionary<string, object> { ["Success"] = false, ["Error"] = "Jogo não encontrado" });
        }
    }

    /// <summary>
    /// Serviço para demonstrações avançadas de SignalR
    /// </summary>
    public class SignalRDemoService : ISignalRDemoService
    {
        private readonly ILogger<SignalRDemoService> _logger;

        public SignalRDemoService(ILogger<SignalRDemoService> logger)
        {
            _logger = logger;
        }

        public Task SendNotificationAsync(string userId, string message)
        {
            _logger.LogInformation("📤 Notificação enviada para {UserId}: {Message}", userId, message);
            return Task.CompletedTask;
        }

        public Task BroadcastMessageAsync(string message)
        {
            _logger.LogInformation("📢 Mensagem broadcast: {Message}", message);
            return Task.CompletedTask;
        }

        public Task SendToGroupAsync(string groupName, string message)
        {
            _logger.LogInformation("👥 Mensagem enviada para grupo {GroupName}: {Message}", groupName, message);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Serviço para chat básico
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly ConcurrentDictionary<string, string> _activeUsers;

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
            _activeUsers = new ConcurrentDictionary<string, string>();
        }

        public Task RegisterUserAsync(string userId, string connectionId)
        {
            _activeUsers.TryAdd(userId, connectionId);
            _logger.LogInformation("👤 Usuário {UserId} registrado com conexão {ConnectionId}", userId, connectionId);
            return Task.CompletedTask;
        }

        public Task UnregisterUserAsync(string userId, string connectionId)
        {
            _activeUsers.TryRemove(userId, out _);
            _logger.LogInformation("👋 Usuário {UserId} removido", userId);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetActiveUsersAsync()
        {
            return Task.FromResult<IEnumerable<string>>(_activeUsers.Keys);
        }
    }
}
