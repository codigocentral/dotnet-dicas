using Microsoft.AspNetCore.SignalR;
using Dica55_SignalR.Models;
using Dica55_SignalR.Services;

namespace Dica55_SignalR.Hubs;

/// <summary>
/// Hub especializado para notificações em tempo real
/// </summary>
public class NotificationHub : Hub
{
    private readonly ISignalRDemoService _demoService;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ISignalRDemoService demoService, ILogger<NotificationHub> logger)
    {
        _demoService = demoService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        var connectionId = Context.ConnectionId;

        // Adicionar aos grupos apropriados
        await Groups.AddToGroupAsync(connectionId, SignalRGroups.Notifications);
        await Groups.AddToGroupAsync(connectionId, SignalRGroups.User(userId));

        // Enviar notificação de conexão
        await _demoService.SendNotificationAsync(userId, "Você está conectado ao sistema de notificações!");

        _logger.LogInformation("🔔 Usuário {UserId} conectado ao hub de notificações", userId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Marcar notificação como lida
    /// </summary>
    public async Task MarkAsRead(string notificationId)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";

        try
        {
            await _demoService.SendNotificationAsync(userId, $"Notificação {notificationId} marcada como lida");
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao marcar notificação {NotificationId} como lida", notificationId);
            await Clients.Caller.SendAsync("Error", $"Erro ao marcar notificação como lida: {ex.Message}");
        }
    }

    /// <summary>
    /// Marcar todas as notificações como lidas
    /// </summary>
    public async Task MarkAllAsRead()
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";

        try
        {
            await _demoService.SendNotificationAsync(userId, "Todas as notificações foram marcadas como lidas");
            await Clients.Caller.SendAsync("AllNotificationsMarkedAsRead");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao marcar todas as notificações como lidas para {UserId}", userId);
            await Clients.Caller.SendAsync("Error", $"Erro ao marcar todas as notificações como lidas: {ex.Message}");
        }
    }

    /// <summary>
    /// Obter histórico de notificações
    /// </summary>
    public async Task GetNotificationHistory(int page = 1, int pageSize = 20)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";

        try
        {
            // Simulação de histórico de notificações
            var notifications = new[]
            {
                new { Id = "1", Message = "Bem-vindo ao sistema!", IsRead = true, Timestamp = DateTime.UtcNow.AddHours(-1) },
                new { Id = "2", Message = "Nova mensagem recebida", IsRead = false, Timestamp = DateTime.UtcNow.AddMinutes(-30) },
                new { Id = "3", Message = "Sistema atualizado", IsRead = true, Timestamp = DateTime.UtcNow.AddMinutes(-15) }
            };
            await Clients.Caller.SendAsync("NotificationHistory", notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao obter histórico de notificações para {UserId}", userId);
            await Clients.Caller.SendAsync("Error", $"Erro ao obter histórico: {ex.Message}");
        }
    }
}

/// <summary>
/// Hub para monitoramento em tempo real de sistema e performance
/// </summary>
public class MonitoringHub : Hub
{
    private readonly IMonitoringService _monitoringService;
    private readonly ILogger<MonitoringHub> _logger;

    public MonitoringHub(IMonitoringService monitoringService, ILogger<MonitoringHub> logger)
    {
        _monitoringService = monitoringService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        var connectionId = Context.ConnectionId;

        // Adicionar ao grupo de monitoramento
        await Groups.AddToGroupAsync(connectionId, SignalRGroups.Monitoring);

        // Enviar dados iniciais
        var currentMetrics = await _monitoringService.GetCurrentMetricsAsync();
        await Clients.Caller.SendAsync("CurrentMetrics", currentMetrics);

        var systemStatus = await _monitoringService.GetSystemStatusAsync();
        await Clients.Caller.SendAsync("SystemStatus", systemStatus);

        _logger.LogInformation("📊 Usuário {UserId} conectado ao hub de monitoramento", userId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Subscrever a métricas específicas
    /// </summary>
    public async Task SubscribeToMetrics(List<string> metricNames)
    {
        var connectionId = Context.ConnectionId;

        foreach (var metricName in metricNames)
        {
            await Groups.AddToGroupAsync(connectionId, $"Metric_{metricName}");
        }

        await Clients.Caller.SendAsync("SubscribedToMetrics", metricNames);
    }

    /// <summary>
    /// Dessubs crever de métricas
    /// </summary>
    public async Task UnsubscribeFromMetrics(List<string> metricNames)
    {
        var connectionId = Context.ConnectionId;

        foreach (var metricName in metricNames)
        {
            await Groups.RemoveFromGroupAsync(connectionId, $"Metric_{metricName}");
        }

        await Clients.Caller.SendAsync("UnsubscribedFromMetrics", metricNames);
    }
}

/// <summary>
/// Hub para colaboração em documentos em tempo real
/// </summary>
public class CollaborationHub : Hub
{
    private readonly ICollaborationService _collaborationService;
    private readonly ILogger<CollaborationHub> _logger;

    public CollaborationHub(ICollaborationService collaborationService, ILogger<CollaborationHub> logger)
    {
        _collaborationService = collaborationService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        _logger.LogInformation("📝 Usuário {UserId} conectado ao hub de colaboração", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        var connectionId = Context.ConnectionId;

        // Remover cursores do usuário de todos os documentos
        await _collaborationService.RemoveUserCursorsAsync(userId);

        _logger.LogInformation("📝 Usuário {UserId} desconectado do hub de colaboração", userId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Entrar em um documento para colaboração
    /// </summary>
    public async Task JoinDocument(string documentId)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        var connectionId = Context.ConnectionId;

        try
        {
            // Verificar permissões
            var canAccess = await _collaborationService.CanAccessDocumentAsync(userId, documentId);
            if (!canAccess)
            {
                await Clients.Caller.SendAsync("Error", "Acesso negado ao documento");
                return;
            }

            // Adicionar ao grupo do documento
            await Groups.AddToGroupAsync(connectionId, SignalRGroups.Document(documentId));

            // Registrar colaborador
            await _collaborationService.AddCollaboratorAsync(documentId, userId);

            // Notificar outros colaboradores
            await Clients.GroupExcept(SignalRGroups.Document(documentId), connectionId)
                         .SendAsync(SignalREvents.CollaboratorJoined, new
                         {
                             DocumentId = documentId,
                             UserId = userId,
                             Timestamp = DateTime.UtcNow
                         });

            // Enviar estado atual do documento
            var document = await _collaborationService.GetDocumentAsync(documentId);
            await Clients.Caller.SendAsync("DocumentState", document);

            // Enviar cursores ativos
            var activeCursors = await _collaborationService.GetActiveCursorsAsync(documentId);
            await Clients.Caller.SendAsync("ActiveCursors", activeCursors);

            _logger.LogInformation("📝 Usuário {UserId} entrou no documento {DocumentId}", userId, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao entrar no documento {DocumentId}", documentId);
            await Clients.Caller.SendAsync("Error", $"Erro ao entrar no documento: {ex.Message}");
        }
    }

    /// <summary>
    /// Sair de um documento
    /// </summary>
    public async Task LeaveDocument(string documentId)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        var connectionId = Context.ConnectionId;

        try
        {
            // Remover do grupo
            await Groups.RemoveFromGroupAsync(connectionId, SignalRGroups.Document(documentId));

            // Remover colaborador
            await _collaborationService.RemoveCollaboratorAsync(documentId, userId);

            // Notificar outros colaboradores
            await Clients.Group(SignalRGroups.Document(documentId))
                         .SendAsync(SignalREvents.CollaboratorLeft, new
                         {
                             DocumentId = documentId,
                             UserId = userId,
                             Timestamp = DateTime.UtcNow
                         });

            _logger.LogInformation("📝 Usuário {UserId} saiu do documento {DocumentId}", userId, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao sair do documento {DocumentId}", documentId);
            await Clients.Caller.SendAsync("Error", $"Erro ao sair do documento: {ex.Message}");
        }
    }

    /// <summary>
    /// Aplicar edição ao documento
    /// </summary>
    public async Task EditDocument(DocumentEditRequest request)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";

        try
        {
            // Aplicar edição
            var edit = new DocumentEdit
            {
                Position = request.Position,
                Content = request.Content,
                Operation = request.Operation,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Length = request.Content?.Length ?? 0
            };
            await _collaborationService.ApplyEditAsync(request.DocumentId, edit);

            // Notificar outros colaboradores
            await Clients.GroupExcept(SignalRGroups.Document(request.DocumentId), Context.ConnectionId)
                         .SendAsync(SignalREvents.DocumentEdited, edit);

            _logger.LogInformation("📝 Documento {DocumentId} editado por {UserId}", request.DocumentId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao editar documento {DocumentId}", request.DocumentId);
            await Clients.Caller.SendAsync("Error", $"Erro ao editar documento: {ex.Message}");
        }
    }

    /// <summary>
    /// Atualizar posição do cursor
    /// </summary>
    public async Task UpdateCursor(CursorUpdateRequest request)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";

        try
        {
            // Atualizar cursor
            await _collaborationService.UpdateCursorAsync(userId, request.DocumentId, request.Position);

            // Notificar outros colaboradores
            await Clients.GroupExcept(SignalRGroups.Document(request.DocumentId), Context.ConnectionId)
                         .SendAsync(SignalREvents.CursorMoved, new
                         {
                             DocumentId = request.DocumentId,
                             UserId = userId,
                             Position = request.Position
                         });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao atualizar cursor no documento {DocumentId}", request.DocumentId);
        }
    }
}

/// <summary>
/// Hub para jogos multiplayer em tempo real
/// </summary>
public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameHub> _logger;

    public GameHub(IGameService gameService, ILogger<GameHub> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        _logger.LogInformation("🎮 Usuário {UserId} conectado ao hub de jogos", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        var connectionId = Context.ConnectionId;

        // Remover jogador de jogos ativos
        await _gameService.HandlePlayerDisconnectionAsync(userId, connectionId);

        _logger.LogInformation("🎮 Usuário {UserId} desconectado do hub de jogos", userId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Entrar em uma sala de jogo
    /// </summary>
    public async Task JoinGame(string gameId)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";
        var connectionId = Context.ConnectionId;

        try
        {
            // Verificar se pode entrar no jogo
            var canJoin = await _gameService.CanJoinGameAsync(userId, gameId);
            if (!canJoin)
            {
                await Clients.Caller.SendAsync("Error", "Não é possível entrar neste jogo");
                return;
            }

            // Adicionar jogador
            await Groups.AddToGroupAsync(connectionId, SignalRGroups.Game(gameId));
            var player = new GamePlayer 
            { 
                Id = userId, 
                Name = $"Player{userId}", 
                ConnectionId = connectionId 
            };
            await _gameService.AddPlayerAsync(gameId, player);

            // Notificar outros jogadores
            await Clients.Group(SignalRGroups.Game(gameId))
                         .SendAsync(SignalREvents.PlayerJoined, player);

            // Enviar estado atual do jogo
            var gameState = await _gameService.GetGameStateAsync(gameId);
            await Clients.Caller.SendAsync("GameState", gameState);

            _logger.LogInformation("🎮 Usuário {UserId} entrou no jogo {GameId}", userId, gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao entrar no jogo {GameId}", gameId);
            await Clients.Caller.SendAsync("Error", $"Erro ao entrar no jogo: {ex.Message}");
        }
    }

    /// <summary>
    /// Fazer uma jogada
    /// </summary>
    public async Task MakeMove(string gameId, Dictionary<string, object> moveData)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";

        try
        {
            // Processar jogada
            var result = await _gameService.ProcessMoveAsync(gameId, userId, moveData);

            // Notificar todos os jogadores
            await Clients.Group(SignalRGroups.Game(gameId))
                         .SendAsync(SignalREvents.PlayerMove, result);

            // Verificar se jogo terminou
            if (result.ContainsKey("gameEnded") && (bool)result["gameEnded"])
            {
                await Clients.Group(SignalRGroups.Game(gameId))
                             .SendAsync(SignalREvents.GameEnded, result);
            }

            _logger.LogInformation("🎮 Jogada feita por {UserId} no jogo {GameId}", userId, gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar jogada no jogo {GameId}", gameId);
            await Clients.Caller.SendAsync("Error", $"Erro ao processar jogada: {ex.Message}");
        }
    }

    /// <summary>
    /// Enviar resposta em quiz
    /// </summary>
    public async Task SubmitAnswer(string gameId, string questionId, int answerIndex)
    {
        var userId = Context.User?.Identity?.Name ?? "Anonymous";

        try
        {
            // Processar resposta
            var result = await _gameService.ProcessQuizAnswerAsync(gameId, userId, questionId, answerIndex);

            // Notificar resultado
            await Clients.Caller.SendAsync(SignalREvents.AnswerSubmitted, result);

            // Atualizar placar se necessário
            if (result.ContainsKey("scoreUpdate"))
            {
                await Clients.Group(SignalRGroups.Game(gameId))
                             .SendAsync(SignalREvents.ScoreUpdate, result["scoreUpdate"]);
            }

            _logger.LogInformation("🎮 Resposta enviada por {UserId} no quiz {GameId}", userId, gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar resposta no quiz {GameId}", gameId);
            await Clients.Caller.SendAsync("Error", $"Erro ao processar resposta: {ex.Message}");
        }
    }
}
