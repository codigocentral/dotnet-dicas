using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Dica78.MicroservicesCommunication.Services;

public class EventStore
{
    private readonly List<EventData> _events = new();
    private readonly ILogger<EventStore> _logger;
    private long _lastEventNumber = 0;

    public EventStore(ILogger<EventStore> logger)
    {
        _logger = logger;
    }

    public async Task<long> AppendEventAsync(string streamId, string eventType, object eventData, object? metadata = null)
    {
        var eventNumber = Interlocked.Increment(ref _lastEventNumber);
        var eventDataJson = JsonSerializer.Serialize(eventData);
        var metadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : null;

        var storedEvent = new EventData
        {
            EventNumber = eventNumber,
            StreamId = streamId,
            EventType = eventType,
            Data = eventDataJson,
            Metadata = metadataJson,
            Timestamp = DateTime.UtcNow
        };

        _events.Add(storedEvent);
        _logger.LogInformation("📝 Evento armazenado: {EventType} #{EventNumber} para stream {StreamId}", 
            eventType, eventNumber, streamId);

        await Task.CompletedTask;
        return eventNumber;
    }

    public async Task<IEnumerable<EventData>> ReadStreamAsync(string streamId, long fromEventNumber = 0)
    {
        await Task.CompletedTask;
        return _events
            .Where(e => e.StreamId == streamId && e.EventNumber >= fromEventNumber)
            .OrderBy(e => e.EventNumber);
    }

    public async Task<IEnumerable<EventData>> ReadAllEventsAsync(long fromEventNumber = 0)
    {
        await Task.CompletedTask;
        return _events
            .Where(e => e.EventNumber >= fromEventNumber)
            .OrderBy(e => e.EventNumber);
    }
}

public class EventSourcingService
{
    private readonly EventStore _eventStore;
    private readonly ILogger<EventSourcingService> _logger;
    private readonly ConcurrentDictionary<string, object> _projections = new();

    public EventSourcingService(EventStore eventStore, ILogger<EventSourcingService> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task DemonstrateEventSourcing()
    {
        await DemonstrateUserAccountEvents();
        await DemonstrateOrderEvents();
        await DemonstrateProjections();
        await DemonstrateEventReplaying();
    }

    private async Task DemonstrateUserAccountEvents()
    {
        Console.WriteLine("👤 1. User Account Event Sourcing");
        Console.WriteLine("---------------------------------");

        var userId = "user-123";
        var streamId = $"user-{userId}";

        // Eventos de criação e atualização de usuário
        await _eventStore.AppendEventAsync(streamId, "UserCreated", new
        {
            UserId = userId,
            Email = "joao@example.com",
            Name = "João Silva",
            CreatedAt = DateTime.UtcNow
        });

        await _eventStore.AppendEventAsync(streamId, "EmailChanged", new
        {
            UserId = userId,
            OldEmail = "joao@example.com",
            NewEmail = "joao.silva@company.com",
            ChangedAt = DateTime.UtcNow
        });

        await _eventStore.AppendEventAsync(streamId, "UserDeactivated", new
        {
            UserId = userId,
            Reason = "User request",
            DeactivatedAt = DateTime.UtcNow
        });

        await _eventStore.AppendEventAsync(streamId, "UserReactivated", new
        {
            UserId = userId,
            ReactivatedAt = DateTime.UtcNow
        });

        // Reconstruir estado atual do usuário
        var userState = await ReconstructUserState(streamId);
        Console.WriteLine($"📊 Estado atual do usuário: {JsonSerializer.Serialize(userState, new JsonSerializerOptions { WriteIndented = true })}");
        Console.WriteLine();
    }

    private async Task DemonstrateOrderEvents()
    {
        Console.WriteLine("🛒 2. Order Event Sourcing");
        Console.WriteLine("--------------------------");

        var orderId = "order-456";
        var streamId = $"order-{orderId}";

        // Eventos do ciclo de vida de um pedido
        await _eventStore.AppendEventAsync(streamId, "OrderCreated", new
        {
            OrderId = orderId,
            CustomerId = "user-123",
            Items = new[]
            {
                new { ProductId = "prod-1", Quantity = 2, Price = 25.99 },
                new { ProductId = "prod-2", Quantity = 1, Price = 49.99 }
            },
            Total = 101.97,
            CreatedAt = DateTime.UtcNow
        });

        await _eventStore.AppendEventAsync(streamId, "PaymentProcessed", new
        {
            OrderId = orderId,
            PaymentId = "pay-789",
            Amount = 101.97,
            Method = "CreditCard",
            ProcessedAt = DateTime.UtcNow
        });

        await _eventStore.AppendEventAsync(streamId, "OrderShipped", new
        {
            OrderId = orderId,
            TrackingNumber = "TRK-12345",
            Carrier = "Express Delivery",
            ShippedAt = DateTime.UtcNow
        });

        await _eventStore.AppendEventAsync(streamId, "OrderDelivered", new
        {
            OrderId = orderId,
            DeliveredAt = DateTime.UtcNow,
            SignedBy = "João Silva"
        });

        // Reconstruir estado do pedido
        var orderState = await ReconstructOrderState(streamId);
        Console.WriteLine($"📦 Estado atual do pedido: {JsonSerializer.Serialize(orderState, new JsonSerializerOptions { WriteIndented = true })}");
        Console.WriteLine();
    }

    private async Task DemonstrateProjections()
    {
        Console.WriteLine("📈 3. Event Projections");
        Console.WriteLine("-----------------------");

        // Projeção de estatísticas de usuários
        var userStats = await CreateUserStatsProjection();
        Console.WriteLine($"👥 Estatísticas de usuários: {JsonSerializer.Serialize(userStats, new JsonSerializerOptions { WriteIndented = true })}");

        // Projeção de vendas
        var salesStats = await CreateSalesProjection();
        Console.WriteLine($"💰 Estatísticas de vendas: {JsonSerializer.Serialize(salesStats, new JsonSerializerOptions { WriteIndented = true })}");
        Console.WriteLine();
    }

    private async Task DemonstrateEventReplaying()
    {
        Console.WriteLine("⏮️  4. Event Replaying");
        Console.WriteLine("---------------------");

        Console.WriteLine("📼 Reproduzindo todos os eventos do sistema:");
        var allEvents = await _eventStore.ReadAllEventsAsync();
        
        foreach (var eventData in allEvents.Take(10)) // Limitar para demonstração
        {
            Console.WriteLine($"  #{eventData.EventNumber}: {eventData.EventType} em {eventData.StreamId} ({eventData.Timestamp:HH:mm:ss})");
        }

        Console.WriteLine($"📊 Total de eventos no sistema: {allEvents.Count()}");
        Console.WriteLine();
    }

    private async Task<object> ReconstructUserState(string streamId)
    {
        var events = await _eventStore.ReadStreamAsync(streamId);
        var state = new
        {
            UserId = "",
            Email = "",
            Name = "",
            IsActive = false,
            CreatedAt = DateTime.MinValue,
            LastModified = DateTime.MinValue
        };

        dynamic currentState = state;

        foreach (var eventData in events)
        {
            var eventObj = JsonSerializer.Deserialize<JsonElement>(eventData.Data);

            switch (eventData.EventType)
            {
                case "UserCreated":
                    currentState = new
                    {
                        UserId = eventObj.GetProperty("UserId").GetString() ?? "",
                        Email = eventObj.GetProperty("Email").GetString() ?? "",
                        Name = eventObj.GetProperty("Name").GetString() ?? "",
                        IsActive = true,
                        CreatedAt = eventObj.GetProperty("CreatedAt").GetDateTime(),
                        LastModified = eventObj.GetProperty("CreatedAt").GetDateTime()
                    };
                    break;

                case "EmailChanged":
                    currentState = new
                    {
                        UserId = currentState.UserId,
                        Email = eventObj.GetProperty("NewEmail").GetString() ?? "",
                        Name = currentState.Name,
                        IsActive = currentState.IsActive,
                        CreatedAt = currentState.CreatedAt,
                        LastModified = eventObj.GetProperty("ChangedAt").GetDateTime()
                    };
                    break;

                case "UserDeactivated":
                    currentState = new
                    {
                        UserId = currentState.UserId,
                        Email = currentState.Email,
                        Name = currentState.Name,
                        IsActive = false,
                        CreatedAt = currentState.CreatedAt,
                        LastModified = eventObj.GetProperty("DeactivatedAt").GetDateTime()
                    };
                    break;

                case "UserReactivated":
                    currentState = new
                    {
                        UserId = currentState.UserId,
                        Email = currentState.Email,
                        Name = currentState.Name,
                        IsActive = true,
                        CreatedAt = currentState.CreatedAt,
                        LastModified = eventObj.GetProperty("ReactivatedAt").GetDateTime()
                    };
                    break;
            }
        }

        return currentState;
    }

    private async Task<object> ReconstructOrderState(string streamId)
    {
        var events = await _eventStore.ReadStreamAsync(streamId);
        var state = new
        {
            OrderId = "",
            CustomerId = "",
            Items = new object[0],
            Total = 0.0,
            Status = "Unknown",
            PaymentId = "",
            TrackingNumber = "",
            CreatedAt = DateTime.MinValue,
            LastModified = DateTime.MinValue
        };

        dynamic currentState = state;

        foreach (var eventData in events)
        {
            var eventObj = JsonSerializer.Deserialize<JsonElement>(eventData.Data);

            switch (eventData.EventType)
            {
                case "OrderCreated":
                    currentState = new
                    {
                        OrderId = eventObj.GetProperty("OrderId").GetString() ?? "",
                        CustomerId = eventObj.GetProperty("CustomerId").GetString() ?? "",
                        Items = JsonSerializer.Deserialize<object[]>(eventObj.GetProperty("Items").GetRawText()) ?? new object[0],
                        Total = eventObj.GetProperty("Total").GetDouble(),
                        Status = "Created",
                        PaymentId = "",
                        TrackingNumber = "",
                        CreatedAt = eventObj.GetProperty("CreatedAt").GetDateTime(),
                        LastModified = eventObj.GetProperty("CreatedAt").GetDateTime()
                    };
                    break;

                case "PaymentProcessed":
                    currentState = new
                    {
                        OrderId = currentState.OrderId,
                        CustomerId = currentState.CustomerId,
                        Items = currentState.Items,
                        Total = currentState.Total,
                        Status = "Paid",
                        PaymentId = eventObj.GetProperty("PaymentId").GetString() ?? "",
                        TrackingNumber = currentState.TrackingNumber,
                        CreatedAt = currentState.CreatedAt,
                        LastModified = eventObj.GetProperty("ProcessedAt").GetDateTime()
                    };
                    break;

                case "OrderShipped":
                    currentState = new
                    {
                        OrderId = currentState.OrderId,
                        CustomerId = currentState.CustomerId,
                        Items = currentState.Items,
                        Total = currentState.Total,
                        Status = "Shipped",
                        PaymentId = currentState.PaymentId,
                        TrackingNumber = eventObj.GetProperty("TrackingNumber").GetString() ?? "",
                        CreatedAt = currentState.CreatedAt,
                        LastModified = eventObj.GetProperty("ShippedAt").GetDateTime()
                    };
                    break;

                case "OrderDelivered":
                    currentState = new
                    {
                        OrderId = currentState.OrderId,
                        CustomerId = currentState.CustomerId,
                        Items = currentState.Items,
                        Total = currentState.Total,
                        Status = "Delivered",
                        PaymentId = currentState.PaymentId,
                        TrackingNumber = currentState.TrackingNumber,
                        CreatedAt = currentState.CreatedAt,
                        LastModified = eventObj.GetProperty("DeliveredAt").GetDateTime()
                    };
                    break;
            }
        }

        return currentState;
    }

    private async Task<object> CreateUserStatsProjection()
    {
        var allEvents = await _eventStore.ReadAllEventsAsync();
        var userEvents = allEvents.Where(e => e.StreamId.StartsWith("user-"));

        var totalUsers = userEvents.Count(e => e.EventType == "UserCreated");
        var activeUsers = 0;
        var emailChanges = userEvents.Count(e => e.EventType == "EmailChanged");

        // Calcular usuários ativos analisando eventos de ativação/desativação
        var userStreams = userEvents.GroupBy(e => e.StreamId);
        foreach (var stream in userStreams)
        {
            var lastStatusEvent = stream
                .Where(e => e.EventType is "UserCreated" or "UserDeactivated" or "UserReactivated")
                .OrderByDescending(e => e.EventNumber)
                .FirstOrDefault();

            if (lastStatusEvent?.EventType is "UserCreated" or "UserReactivated")
                activeUsers++;
        }

        return new
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = totalUsers - activeUsers,
            EmailChanges = emailChanges
        };
    }

    private async Task<object> CreateSalesProjection()
    {
        var allEvents = await _eventStore.ReadAllEventsAsync();
        var orderEvents = allEvents.Where(e => e.StreamId.StartsWith("order-"));

        var totalOrders = orderEvents.Count(e => e.EventType == "OrderCreated");
        var paidOrders = orderEvents.Count(e => e.EventType == "PaymentProcessed");
        var shippedOrders = orderEvents.Count(e => e.EventType == "OrderShipped");
        var deliveredOrders = orderEvents.Count(e => e.EventType == "OrderDelivered");

        var totalRevenue = 0.0;
        foreach (var orderCreated in orderEvents.Where(e => e.EventType == "OrderCreated"))
        {
            var eventObj = JsonSerializer.Deserialize<JsonElement>(orderCreated.Data);
            totalRevenue += eventObj.GetProperty("Total").GetDouble();
        }

        return new
        {
            TotalOrders = totalOrders,
            PaidOrders = paidOrders,
            ShippedOrders = shippedOrders,
            DeliveredOrders = deliveredOrders,
            TotalRevenue = totalRevenue,
            ConversionRate = totalOrders > 0 ? Math.Round((double)deliveredOrders / totalOrders * 100, 2) : 0
        };
    }
}

public class EventData
{
    public long EventNumber { get; set; }
    public string StreamId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public DateTime Timestamp { get; set; }
}
