using System.ComponentModel.DataAnnotations;

namespace Dica55_SignalR.Models;

// ==========================================
// MODELOS DE CHAT
// ==========================================

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string User { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Room { get; set; } = "general";
    public MessageType Type { get; set; } = MessageType.Text;
    public MessageType MessageType { get; set; } = MessageType.Text;
    public string? ReplyToId { get; set; }
    public List<string> Mentions { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ChatRoom
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsPrivate { get; set; } = false;
    public List<string> Members { get; set; } = new();
    public List<string> Admins { get; set; } = new();
    public int MessageCount { get; set; } = 0;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

public class ChatUser
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Online;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public List<string> Rooms { get; set; } = new();
    public string ConnectionId { get; set; } = string.Empty;
    public Dictionary<string, object> Profile { get; set; } = new();
}

// ==========================================
// MODELOS DE NOTIFICAÇÃO
// ==========================================

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public string? ActionUrl { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class BroadcastMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public BroadcastType Type { get; set; } = BroadcastType.Announcement;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public List<string> TargetUsers { get; set; } = new();
    public List<string> TargetRoles { get; set; } = new();
    public bool IsGlobal { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
}

// ==========================================
// MODELOS DE MONITORAMENTO
// ==========================================

public class LiveMetric
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
    public MetricType Type { get; set; } = MetricType.Counter;
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class SystemStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public ServiceHealth Health { get; set; } = ServiceHealth.Healthy;
    public string Version { get; set; } = string.Empty;
    public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    public TimeSpan Uptime { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
    public List<string> Issues { get; set; } = new();
}

public class PerformanceData
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public int ActiveConnections { get; set; }
    public int RequestsPerSecond { get; set; }
    public double ResponseTime { get; set; }
    public int ErrorRate { get; set; }
}

// ==========================================
// MODELOS DE COLABORAÇÃO
// ==========================================

public class CollaborativeDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
    public List<string> Collaborators { get; set; } = new();
    public List<DocumentEdit> EditHistory { get; set; } = new();
    public Dictionary<string, UserCursor> ActiveCursors { get; set; } = new();
    public bool IsLocked { get; set; } = false;
    public string? LockedBy { get; set; }
}

public class DocumentEdit
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public EditOperation Operation { get; set; } = EditOperation.Insert;
    public int Position { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Length { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class UserCursor
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public int Position { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}

// ==========================================
// MODELOS DE JOGOS
// ==========================================

public class GameRoom
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public GameType Type { get; set; } = GameType.Quiz;
    public GameStatus Status { get; set; } = GameStatus.Waiting;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public int MaxPlayers { get; set; } = 10;
    public List<GamePlayer> Players { get; set; } = new();
    public Dictionary<string, object> GameData { get; set; } = new();
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? WinnerId { get; set; }
}

public class GamePlayer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; } = 0;
    public PlayerStatus Status { get; set; } = PlayerStatus.Waiting;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public string ConnectionId { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

public class QuizQuestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswer { get; set; }
    public int TimeLimit { get; set; } = 30; // segundos
    public int Points { get; set; } = 10;
    public string Category { get; set; } = string.Empty;
}

// ==========================================
// DTOs PARA REQUESTS/RESPONSES
// ==========================================

public class SendMessageRequest
{
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string Room { get; set; } = "general";
    public string? ReplyToId { get; set; }
    public List<string> Mentions { get; set; } = new();
}

public class JoinRoomRequest
{
    [Required]
    public string RoomId { get; set; } = string.Empty;
    
    public string? Password { get; set; }
}

public class CreateRoomRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public bool IsPrivate { get; set; } = false;
    public string? Password { get; set; }
}

public class SendNotificationRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; } = NotificationType.Info;
    public string? UserId { get; set; }
    public string? ActionUrl { get; set; }
}

public class DocumentEditRequest
{
    [Required]
    public string DocumentId { get; set; } = string.Empty;
    
    [Required]
    public EditOperation Operation { get; set; }
    
    public int Position { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class CursorUpdateRequest
{
    [Required]
    public string DocumentId { get; set; } = string.Empty;
    
    public int Position { get; set; }
}

// ==========================================
// ENUMS
// ==========================================

public enum MessageType
{
    Text,
    Image,
    File,
    System,
    Emoji,
    Command
}

public enum UserStatus
{
    Online,
    Away,
    Busy,
    Offline
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    System
}

public enum BroadcastType
{
    Announcement,
    Alert,
    News,
    Maintenance,
    Emergency
}

public enum MetricType
{
    Counter,
    Gauge,
    Histogram,
    Summary
}

public enum ServiceHealth
{
    Healthy,
    Degraded,
    Unhealthy,
    Unknown
}

public enum EditOperation
{
    Insert,
    Delete,
    Replace,
    Format
}

public enum GameType
{
    Quiz,
    TicTacToe,
    Chess,
    Checkers,
    Custom,
    Multiplayer
}

public enum GameStatus
{
    Waiting,
    Starting,
    InProgress,
    Paused,
    Finished,
    Cancelled,
    Ended
}

public enum PlayerStatus
{
    Waiting,
    Ready,
    Playing,
    Spectating,
    Disconnected
}

// ==========================================
// EVENTOS SIGNALR
// ==========================================

public static class SignalREvents
{
    // Chat Events
    public const string ReceiveMessage = "ReceiveMessage";
    public const string UserJoined = "UserJoined";
    public const string UserLeft = "UserLeft";
    public const string UserTyping = "UserTyping";
    public const string UserStoppedTyping = "UserStoppedTyping";
    public const string RoomCreated = "RoomCreated";
    public const string RoomUpdated = "RoomUpdated";
    
    // Notification Events
    public const string ReceiveNotification = "ReceiveNotification";
    public const string BroadcastMessage = "BroadcastMessage";
    public const string SystemAlert = "SystemAlert";
    
    // Monitoring Events
    public const string MetricUpdate = "MetricUpdate";
    public const string StatusUpdate = "StatusUpdate";
    public const string PerformanceUpdate = "PerformanceUpdate";
    
    // Collaboration Events
    public const string DocumentEdited = "DocumentEdited";
    public const string CursorMoved = "CursorMoved";
    public const string DocumentLocked = "DocumentLocked";
    public const string DocumentUnlocked = "DocumentUnlocked";
    public const string CollaboratorJoined = "CollaboratorJoined";
    public const string CollaboratorLeft = "CollaboratorLeft";
    
    // Game Events
    public const string GameStarted = "GameStarted";
    public const string GameEnded = "GameEnded";
    public const string PlayerJoined = "PlayerJoined";
    public const string PlayerLeft = "PlayerLeft";
    public const string PlayerMove = "PlayerMove";
    public const string ScoreUpdate = "ScoreUpdate";
    public const string QuestionPresented = "QuestionPresented";
    public const string AnswerSubmitted = "AnswerSubmitted";
    public const string RoundEnded = "RoundEnded";
    
    // Connection Events
    public const string ConnectionEstablished = "ConnectionEstablished";
    public const string ConnectionLost = "ConnectionLost";
    public const string Reconnected = "Reconnected";
}

// ==========================================
// CONSTANTES
// ==========================================

public static class SignalRGroups
{
    public const string AllUsers = "AllUsers";
    public const string Administrators = "Administrators";
    public const string Moderators = "Moderators";
    public const string Monitoring = "Monitoring";
    public const string Notifications = "Notifications";
    
    public static string ChatRoom(string roomId) => $"ChatRoom_{roomId}";
    public static string Document(string documentId) => $"Document_{documentId}";
    public static string Game(string gameId) => $"Game_{gameId}";
    public static string User(string userId) => $"User_{userId}";
}
