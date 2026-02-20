using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Dica78.MicroservicesCommunication.Services;

public class MessageBroker
{
    private readonly ILogger<MessageBroker> _logger;
    private readonly ConcurrentDictionary<string, List<Func<Message, Task>>> _subscribers = new();
    private readonly ConcurrentQueue<Message> _messageQueue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public MessageBroker(ILogger<MessageBroker> logger)
    {
        _logger = logger;
        StartMessageProcessor();
    }

    public async Task PublishAsync(string topic, object data)
    {
        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            Topic = topic,
            Data = JsonSerializer.Serialize(data),
            Timestamp = DateTime.UtcNow
        };

        _messageQueue.Enqueue(message);
        _logger.LogInformation("📤 Mensagem publicada: {Topic} - {Id}", topic, message.Id);
        await Task.CompletedTask;
    }

    public void Subscribe(string topic, Func<Message, Task> handler)
    {
        _subscribers.AddOrUpdate(topic, 
            [handler], 
            (key, existing) => 
            {
                existing.Add(handler);
                return existing;
            });

        _logger.LogInformation("📥 Novo subscriber para tópico: {Topic}", topic);
    }

    private void StartMessageProcessor()
    {
        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var message))
                {
                    await ProcessMessage(message);
                }
                else
                {
                    await Task.Delay(100, _cancellationTokenSource.Token);
                }
            }
        }, _cancellationTokenSource.Token);
    }

    private async Task ProcessMessage(Message message)
    {
        if (_subscribers.TryGetValue(message.Topic, out var handlers))
        {
            var tasks = handlers.Select(handler => 
                ExecuteHandlerSafely(handler, message));
            
            await Task.WhenAll(tasks);
        }
    }

    private async Task ExecuteHandlerSafely(Func<Message, Task> handler, Message message)
    {
        try
        {
            await handler(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem {MessageId}", message.Id);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}

public class MessageQueueService
{
    private readonly MessageBroker _messageBroker;
    private readonly ILogger<MessageQueueService> _logger;

    public MessageQueueService(MessageBroker messageBroker, ILogger<MessageQueueService> logger)
    {
        _messageBroker = messageBroker;
        _logger = logger;
    }

    public async Task DemonstrateMessagePatterns()
    {
        await DemonstratePublishSubscribe();
        await DemonstrateRequestReply();
        await DemonstrateEventDrivenArchitecture();
    }

    private async Task DemonstratePublishSubscribe()
    {
        Console.WriteLine("📡 1. Publish/Subscribe Pattern");
        Console.WriteLine("-------------------------------");

        // Configurar subscribers
        _messageBroker.Subscribe("user.created", async message =>
        {
            var userData = JsonSerializer.Deserialize<JsonElement>(message.Data);
            Console.WriteLine($"📧 Email Service: Enviando boas-vindas para {userData.GetProperty("email").GetString()}");
            await Task.Delay(100); // Simula processamento
        });

        _messageBroker.Subscribe("user.created", async message =>
        {
            var userData = JsonSerializer.Deserialize<JsonElement>(message.Data);
            Console.WriteLine($"📊 Analytics Service: Novo usuário registrado - {userData.GetProperty("name").GetString()}");
            await Task.Delay(50);
        });

        _messageBroker.Subscribe("user.created", async message =>
        {
            Console.WriteLine("🎁 Promotion Service: Criando ofertas de boas-vindas");
            await Task.Delay(75);
        });

        // Publicar evento
        var newUser = new { id = 1, name = "João Silva", email = "joao@example.com" };
        await _messageBroker.PublishAsync("user.created", newUser);

        await Task.Delay(500); // Aguarda processamento
        Console.WriteLine("✅ Evento processado por todos os subscribers\n");
    }

    private async Task DemonstrateRequestReply()
    {
        Console.WriteLine("🔄 2. Request/Reply Pattern");
        Console.WriteLine("---------------------------");

        var replyReceived = false;
        var correlationId = Guid.NewGuid().ToString();

        // Configurar reply handler
        _messageBroker.Subscribe($"order.reply.{correlationId}", async message =>
        {
            var orderData = JsonSerializer.Deserialize<JsonElement>(message.Data);
            Console.WriteLine($"✅ Reply recebido: Pedido {orderData.GetProperty("orderId").GetString()} processado");
            replyReceived = true;
            await Task.CompletedTask;
        });

        // Configurar request handler
        _messageBroker.Subscribe("order.request", async message =>
        {
            var requestData = JsonSerializer.Deserialize<JsonElement>(message.Data);
            var replyTopic = requestData.GetProperty("replyTo").GetString();
            
            Console.WriteLine("🔄 Order Service: Processando pedido...");
            await Task.Delay(200); // Simula processamento
            
            var reply = new { orderId = "ORD-12345", status = "processed", total = 99.99 };
            await _messageBroker.PublishAsync(replyTopic!, reply);
        });

        // Enviar request
        var orderRequest = new 
        { 
            productId = 123, 
            quantity = 2, 
            replyTo = $"order.reply.{correlationId}" 
        };
        
        await _messageBroker.PublishAsync("order.request", orderRequest);
        Console.WriteLine("📤 Request enviado, aguardando reply...");

        // Aguardar reply
        var timeout = DateTime.UtcNow.AddSeconds(2);
        while (!replyReceived && DateTime.UtcNow < timeout)
        {
            await Task.Delay(50);
        }

        Console.WriteLine(replyReceived ? "✅ Request/Reply concluído\n" : "⏰ Timeout na resposta\n");
    }

    private async Task DemonstrateEventDrivenArchitecture()
    {
        Console.WriteLine("🏗️  3. Event-Driven Architecture");
        Console.WriteLine("---------------------------------");

        // Configurar event handlers para workflow
        _messageBroker.Subscribe("payment.completed", async message =>
        {
            Console.WriteLine("💳 Payment completed - Iniciando fulfillment");
            await Task.Delay(100);
            
            var paymentData = JsonSerializer.Deserialize<JsonElement>(message.Data);
            var fulfillmentEvent = new 
            { 
                orderId = paymentData.GetProperty("orderId").GetString(),
                action = "ship_order"
            };
            
            await _messageBroker.PublishAsync("fulfillment.requested", fulfillmentEvent);
        });

        _messageBroker.Subscribe("fulfillment.requested", async message =>
        {
            Console.WriteLine("📦 Fulfillment requested - Preparando envio");
            await Task.Delay(150);
            
            var fulfillmentData = JsonSerializer.Deserialize<JsonElement>(message.Data);
            var shippingEvent = new 
            { 
                orderId = fulfillmentData.GetProperty("orderId").GetString(),
                trackingId = "TRK-" + Random.Shared.Next(10000, 99999)
            };
            
            await _messageBroker.PublishAsync("shipping.started", shippingEvent);
        });

        _messageBroker.Subscribe("shipping.started", async message =>
        {
            var shippingData = JsonSerializer.Deserialize<JsonElement>(message.Data);
            Console.WriteLine($"🚚 Shipping started - Tracking: {shippingData.GetProperty("trackingId").GetString()}");
            await Task.Delay(75);
            
            await _messageBroker.PublishAsync("notification.send", new 
            { 
                type = "shipping_notification",
                orderId = shippingData.GetProperty("orderId").GetString(),
                trackingId = shippingData.GetProperty("trackingId").GetString()
            });
        });

        _messageBroker.Subscribe("notification.send", async message =>
        {
            var notificationData = JsonSerializer.Deserialize<JsonElement>(message.Data);
            Console.WriteLine($"📧 Notification sent: {notificationData.GetProperty("type").GetString()}");
            await Task.Delay(50);
        });

        // Iniciar workflow com evento de pagamento
        var paymentEvent = new { orderId = "ORD-67890", amount = 149.99, currency = "USD" };
        await _messageBroker.PublishAsync("payment.completed", paymentEvent);

        await Task.Delay(1000); // Aguarda o workflow completo
        Console.WriteLine("✅ Event-driven workflow concluído\n");
    }
}

public class Message
{
    public string Id { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}
