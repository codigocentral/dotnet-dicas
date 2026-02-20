using Microsoft.AspNetCore.SignalR;
using Dica55_SignalR.Services;

namespace Dica55_SignalR.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IChatService _chatService;

    public ChatHub(ILogger<ChatHub> logger, IChatService chatService)
    {
        _logger = logger;
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.ConnectionId; // Simplificado para usar ConnectionId
        await _chatService.RegisterUserAsync(userId, Context.ConnectionId);
        
        await Clients.All.SendAsync("UserJoined", new 
        { 
            userId = userId, 
            message = $"Usuário {userId} entrou no chat",
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("👤 Usuário conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.ConnectionId;
        await _chatService.UnregisterUserAsync(userId, Context.ConnectionId);
        
        await Clients.Others.SendAsync("UserLeft", new 
        { 
            userId = userId, 
            message = $"Usuário {userId} saiu do chat",
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("👋 Usuário desconectado: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message)
    {
        var userId = Context.ConnectionId;
        
        var chatMessage = new 
        {
            id = Guid.NewGuid().ToString(),
            user = userId,
            message = message,
            timestamp = DateTime.UtcNow
        };

        // Broadcast para todos os clientes
        await Clients.All.SendAsync("ReceiveMessage", chatMessage);

        _logger.LogInformation("💬 Mensagem enviada por {UserId}: {Message}", userId, message);
    }

    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        
        await Clients.Group(roomName).SendAsync("RoomMessage", new 
        { 
            message = $"Usuário {Context.ConnectionId} entrou na sala {roomName}",
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("🚪 Usuário {ConnectionId} entrou na sala {RoomName}", Context.ConnectionId, roomName);
    }

    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        
        await Clients.Group(roomName).SendAsync("RoomMessage", new 
        { 
            message = $"Usuário {Context.ConnectionId} saiu da sala {roomName}",
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("🚪 Usuário {ConnectionId} saiu da sala {RoomName}", Context.ConnectionId, roomName);
    }

    public async Task SendToRoom(string roomName, string message)
    {
        var chatMessage = new 
        {
            id = Guid.NewGuid().ToString(),
            user = Context.ConnectionId,
            message = message,
            room = roomName,
            timestamp = DateTime.UtcNow
        };

        await Clients.Group(roomName).SendAsync("ReceiveMessage", chatMessage);

        _logger.LogInformation("💬 Mensagem enviada para sala {RoomName} por {UserId}: {Message}", 
            roomName, Context.ConnectionId, message);
    }
}
