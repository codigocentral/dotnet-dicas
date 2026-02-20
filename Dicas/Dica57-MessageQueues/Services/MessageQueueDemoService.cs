using Microsoft.Extensions.Logging;
using Dica57.MessageQueues.Models;

namespace Dica57.MessageQueues.Services;

/// <summary>
/// Serviço principal para demonstrações de Message Queues
/// </summary>
public class MessageQueueDemoService
{
    private readonly ILogger<MessageQueueDemoService> _logger;
    private readonly InMemoryMessageBroker _inMemoryBroker;
    private readonly RabbitMQService _rabbitMQService;
    private readonly AzureServiceBusService _serviceBusService;

    public MessageQueueDemoService(
        ILogger<MessageQueueDemoService> logger,
        InMemoryMessageBroker inMemoryBroker,
        RabbitMQService rabbitMQService,
        AzureServiceBusService serviceBusService)
    {
        _logger = logger;
        _inMemoryBroker = inMemoryBroker;
        _rabbitMQService = rabbitMQService;
        _serviceBusService = serviceBusService;
    }

    /// <summary>
    /// Executa todas as demonstrações de Message Queues
    /// </summary>
    public async Task RunAllDemonstrations()
    {
        Console.WriteLine("🚀 Iniciando demonstrações de Message Queues...\n");

        await DemonstrateInMemoryBroker();
        await WaitForKeyPress("Pressione qualquer tecla para continuar com RabbitMQ...");

        await _rabbitMQService.DemonstrateRabbitMQPatterns();
        await WaitForKeyPress("Pressione qualquer tecla para continuar com Azure Service Bus...");

        await _serviceBusService.DemonstrateServiceBusPatterns();
        await WaitForKeyPress("Pressione qualquer tecla para ver comparações e boas práticas...");

        await DemonstrateMessagePatterns();
        ShowBestPractices();
        ShowPerformanceComparison();
    }

    private async Task DemonstrateInMemoryBroker()
    {
        Console.WriteLine("💾 1. In-Memory Message Broker");
        Console.WriteLine("==============================");

        await DemonstrateBasicPubSub();
        await DemonstrateMultipleSubscribers();
        await DemonstrateMessageTypes();
        
        var stats = _inMemoryBroker.GetStatistics();
        Console.WriteLine($"📊 Estatísticas: {stats.MessagesProduced} produzidas, {stats.MessagesConsumed} consumidas\n");
    }

    private async Task DemonstrateBasicPubSub()
    {
        Console.WriteLine("📡 Basic Publish/Subscribe");
        Console.WriteLine("--------------------------");

        _inMemoryBroker.Subscribe<OrderMessage>("orders", async order =>
        {
            Console.WriteLine($"📦 Order received: {order.OrderId} - {order.CustomerName} - ${order.TotalAmount}");
            await Task.Delay(100);
        });

        var orders = new[]
        {
            new OrderMessage { OrderId = 1, CustomerName = "João Silva", TotalAmount = 299.99m },
            new OrderMessage { OrderId = 2, CustomerName = "Maria Santos", TotalAmount = 150.00m },
            new OrderMessage { OrderId = 3, CustomerName = "Pedro Costa", TotalAmount = 75.50m }
        };

        foreach (var order in orders)
        {
            await _inMemoryBroker.PublishAsync("orders", order);
            await Task.Delay(200);
        }

        await Task.Delay(500);
        Console.WriteLine("✅ Basic Pub/Sub completed\n");
    }

    private async Task DemonstrateMultipleSubscribers()
    {
        Console.WriteLine("👥 Multiple Subscribers");
        Console.WriteLine("-----------------------");

        // Múltiplos handlers para o mesmo tópico
        _inMemoryBroker.Subscribe<UserEventMessage>("user.registered", async user =>
        {
            Console.WriteLine($"📧 Email Service: Sending welcome email to {user.Email}");
            await Task.Delay(50);
        });

        _inMemoryBroker.Subscribe<UserEventMessage>("user.registered", async user =>
        {
            Console.WriteLine($"📊 Analytics: New user registered - {user.UserName}");
            await Task.Delay(30);
        });

        _inMemoryBroker.Subscribe<UserEventMessage>("user.registered", async user =>
        {
            Console.WriteLine($"🎁 Promotions: Creating welcome offers for {user.UserName}");
            await Task.Delay(75);
        });

        _inMemoryBroker.Subscribe<UserEventMessage>("user.registered", async user =>
        {
            Console.WriteLine($"🔔 Notifications: Setting up preferences for {user.UserName}");
            await Task.Delay(40);
        });

        var newUser = new UserEventMessage
        {
            UserId = 1,
            UserName = "Ana Silva",
            Email = "ana@example.com",
            EventType = "Created"
        };

        await _inMemoryBroker.PublishAsync("user.registered", newUser);
        await Task.Delay(300);
        Console.WriteLine("✅ Multiple Subscribers completed\n");
    }

    private async Task DemonstrateMessageTypes()
    {
        Console.WriteLine("📝 Different Message Types");
        Console.WriteLine("--------------------------");

        // Handler para notificações
        _inMemoryBroker.Subscribe<NotificationMessage>("notifications", async notification =>
        {
            var icon = notification.Type switch
            {
                NotificationType.Email => "📧",
                NotificationType.SMS => "📱",
                NotificationType.Push => "🔔",
                NotificationType.Slack => "💬",
                NotificationType.Teams => "👥",
                _ => "📝"
            };

            Console.WriteLine($"{icon} {notification.Type}: {notification.Subject} -> {notification.To}");
            await Task.Delay(50);
        });

        // Handler para métricas
        _inMemoryBroker.Subscribe<SystemMetricMessage>("metrics", async metric =>
        {
            Console.WriteLine($"📊 Metric: {metric.ServiceName}.{metric.MetricName} = {metric.Value} {metric.Unit}");
            await Task.Delay(30);
        });

        // Envia diferentes tipos de mensagens
        var messages = new BaseMessage[]
        {
            new NotificationMessage 
            { 
                To = "user@test.com", 
                Subject = "Order Confirmed", 
                Type = NotificationType.Email,
                Priority = NotificationPriority.Normal
            },
            new NotificationMessage 
            { 
                To = "+1234567890", 
                Subject = "Delivery Update", 
                Type = NotificationType.SMS,
                Priority = NotificationPriority.High
            },
            new SystemMetricMessage 
            { 
                ServiceName = "api-gateway", 
                MetricName = "requests_per_second", 
                Value = 1250, 
                Unit = "req/s" 
            },
            new SystemMetricMessage 
            { 
                ServiceName = "database", 
                MetricName = "connection_pool_usage", 
                Value = 85.5, 
                Unit = "%" 
            }
        };

        foreach (var message in messages)
        {
            switch (message)
            {
                case NotificationMessage notification:
                    await _inMemoryBroker.PublishAsync("notifications", notification);
                    break;
                case SystemMetricMessage metric:
                    await _inMemoryBroker.PublishAsync("metrics", metric);
                    break;
            }
            await Task.Delay(150);
        }

        await Task.Delay(500);
        Console.WriteLine("✅ Different Message Types completed\n");
    }

    private async Task DemonstrateMessagePatterns()
    {
        Console.WriteLine("🎯 4. Message Patterns Summary");
        Console.WriteLine("==============================");

        await DemonstrateFireAndForget();
        await DemonstrateRequestReply();
        await DemonstrateEventSourcing();
    }

    private async Task DemonstrateFireAndForget()
    {
        Console.WriteLine("🚀 Fire-and-Forget Pattern");
        Console.WriteLine("---------------------------");

        _inMemoryBroker.Subscribe<OrderMessage>("fire-forget.orders", async order =>
        {
            Console.WriteLine($"🔥 Fire-and-Forget: Processing order {order.OrderId} asynchronously");
            await Task.Delay(Random.Shared.Next(50, 200));
            Console.WriteLine($"✅ Order {order.OrderId} processed (no response expected)");
        });

        for (int i = 1; i <= 3; i++)
        {
            var order = new OrderMessage 
            { 
                OrderId = i, 
                CustomerName = $"Customer {i}", 
                TotalAmount = Random.Shared.Next(100, 500) 
            };

            await _inMemoryBroker.PublishAsync("fire-forget.orders", order);
            Console.WriteLine($"📤 Order {i} sent (fire-and-forget)");
            
            // Não aguarda resposta - continua imediatamente
            await Task.Delay(100);
        }

        await Task.Delay(1000);
        Console.WriteLine("✅ Fire-and-Forget completed\n");
    }

    private async Task DemonstrateRequestReply()
    {
        Console.WriteLine("🔄 Request-Reply Pattern");
        Console.WriteLine("------------------------");

        var responses = new Dictionary<string, PaymentResponseMessage>();

        // Simula serviço de pagamento que responde a requests
        _inMemoryBroker.Subscribe<OrderMessage>("payment.requests", async order =>
        {
            Console.WriteLine($"💳 Payment Service: Processing payment for order {order.OrderId}");
            await Task.Delay(200); // Simula processamento

            var success = Random.Shared.NextDouble() > 0.2; // 80% de sucesso
            var response = new PaymentResponseMessage
            {
                OrderId = order.OrderId,
                Amount = order.TotalAmount,
                Status = success ? PaymentStatus.Completed : PaymentStatus.Failed,
                TransactionId = success ? $"TXN{Random.Shared.Next(10000, 99999)}" : "",
                ErrorMessage = success ? "" : "Insufficient funds",
                CorrelationId = order.CorrelationId
            };

            if (!string.IsNullOrEmpty(order.ReplyTo))
            {
                await _inMemoryBroker.PublishAsync(order.ReplyTo, response);
            }
        });

        // Cliente que envia requests e aguarda replies
        for (int i = 1; i <= 3; i++)
        {
            var correlationId = Guid.NewGuid().ToString();
            var replyTopic = $"payment.replies.{correlationId}";
            var responseReceived = false;

            // Configura handler para resposta específica
            _inMemoryBroker.Subscribe<PaymentResponseMessage>(replyTopic, async response =>
            {
                responses[response.CorrelationId] = response;
                var statusIcon = response.Status == PaymentStatus.Completed ? "✅" : "❌";
                Console.WriteLine($"{statusIcon} Payment Response for order {response.OrderId}: {response.Status}");
                
                if (response.Status == PaymentStatus.Failed)
                {
                    Console.WriteLine($"   Error: {response.ErrorMessage}");
                }
                else
                {
                    Console.WriteLine($"   Transaction ID: {response.TransactionId}");
                }
                
                responseReceived = true;
                await Task.CompletedTask;
            });

            // Envia request
            var order = new OrderMessage
            {
                OrderId = i,
                CustomerName = $"Customer {i}",
                TotalAmount = Random.Shared.Next(100, 500),
                CorrelationId = correlationId,
                ReplyTo = replyTopic
            };

            Console.WriteLine($"📤 Sending payment request for order {i}...");
            await _inMemoryBroker.PublishAsync("payment.requests", order);

            // Aguarda resposta (com timeout)
            var timeout = DateTime.UtcNow.AddSeconds(2);
            while (!responseReceived && DateTime.UtcNow < timeout)
            {
                await Task.Delay(50);
            }

            if (!responseReceived)
            {
                Console.WriteLine($"⏰ Timeout waiting for payment response for order {i}");
            }

            await Task.Delay(300);
        }

        Console.WriteLine($"✅ Request-Reply completed. {responses.Count} responses received\n");
    }

    private async Task DemonstrateEventSourcing()
    {
        Console.WriteLine("📚 Event Sourcing Pattern");
        Console.WriteLine("-------------------------");

        var eventStore = new List<BaseMessage>();

        // Event handlers que armazenam todos os eventos
        _inMemoryBroker.Subscribe<OrderMessage>("events.orders", async orderEvent =>
        {
            eventStore.Add(orderEvent);
            Console.WriteLine($"📝 Event stored: Order {orderEvent.OrderId} - {orderEvent.Status}");
            await Task.CompletedTask;
        });

        _inMemoryBroker.Subscribe<PaymentResponseMessage>("events.payments", async paymentEvent =>
        {
            eventStore.Add(paymentEvent);
            Console.WriteLine($"📝 Event stored: Payment {paymentEvent.OrderId} - {paymentEvent.Status}");
            await Task.CompletedTask;
        });

        _inMemoryBroker.Subscribe<UserEventMessage>("events.users", async userEvent =>
        {
            eventStore.Add(userEvent);
            Console.WriteLine($"📝 Event stored: User {userEvent.UserId} - {userEvent.EventType}");
            await Task.CompletedTask;
        });

        // Simula sequência de eventos
        var events = new BaseMessage[]
        {
            new UserEventMessage { UserId = 1, UserName = "Alice", EventType = "Created" },
            new OrderMessage { OrderId = 1, CustomerId = "CUST001", CustomerName = "Alice", Status = "Created" },
            new PaymentResponseMessage { OrderId = 1, Status = PaymentStatus.Completed, Amount = 199.99m },
            new OrderMessage { OrderId = 1, CustomerId = "CUST001", CustomerName = "Alice", Status = "Paid" },
            new OrderMessage { OrderId = 1, CustomerId = "CUST001", CustomerName = "Alice", Status = "Shipped" },
            new OrderMessage { OrderId = 1, CustomerId = "CUST001", CustomerName = "Alice", Status = "Delivered" }
        };

        foreach (var evt in events)
        {
            var topic = evt switch
            {
                OrderMessage => "events.orders",
                PaymentResponseMessage => "events.payments",
                UserEventMessage => "events.users",
                _ => "events.unknown"
            };

            await _inMemoryBroker.PublishAsync(topic, evt);
            await Task.Delay(150);
        }

        await Task.Delay(500);

        Console.WriteLine($"\n📊 Event Store Summary:");
        Console.WriteLine($"   Total events: {eventStore.Count}");
        Console.WriteLine($"   Order events: {eventStore.OfType<OrderMessage>().Count()}");
        Console.WriteLine($"   Payment events: {eventStore.OfType<PaymentResponseMessage>().Count()}");
        Console.WriteLine($"   User events: {eventStore.OfType<UserEventMessage>().Count()}");
        
        Console.WriteLine("✅ Event Sourcing completed\n");
    }

    private void ShowBestPractices()
    {
        Console.WriteLine("💡 5. Best Practices");
        Console.WriteLine("====================");

        Console.WriteLine("🔧 Design Patterns:");
        Console.WriteLine("   ✅ Use idempotent message handlers");
        Console.WriteLine("   ✅ Implement dead letter queues for failed messages");
        Console.WriteLine("   ✅ Use correlation IDs for request/reply patterns");
        Console.WriteLine("   ✅ Include metadata (timestamps, source, version)");
        Console.WriteLine("   ✅ Design for eventual consistency");

        Console.WriteLine("\n⚡ Performance:");
        Console.WriteLine("   ✅ Batch messages when possible");
        Console.WriteLine("   ✅ Use appropriate message size (avoid large payloads)");
        Console.WriteLine("   ✅ Configure proper timeout values");
        Console.WriteLine("   ✅ Monitor queue depth and processing times");
        Console.WriteLine("   ✅ Implement circuit breakers for external dependencies");

        Console.WriteLine("\n🔒 Security:");
        Console.WriteLine("   ✅ Encrypt sensitive message data");
        Console.WriteLine("   ✅ Use authentication and authorization");
        Console.WriteLine("   ✅ Validate and sanitize message content");
        Console.WriteLine("   ✅ Implement proper access controls");
        Console.WriteLine("   ✅ Audit message flows");

        Console.WriteLine("\n🔄 Reliability:");
        Console.WriteLine("   ✅ Use durable queues for critical messages");
        Console.WriteLine("   ✅ Implement retry policies with exponential backoff");
        Console.WriteLine("   ✅ Handle duplicate messages gracefully");
        Console.WriteLine("   ✅ Monitor and alert on queue health");
        Console.WriteLine("   ✅ Plan for disaster recovery");

        Console.WriteLine();
    }

    private void ShowPerformanceComparison()
    {
        Console.WriteLine("📊 6. Performance Comparison");
        Console.WriteLine("============================");

        Console.WriteLine("🏆 RabbitMQ:");
        Console.WriteLine("   • Throughput: 10K-100K+ messages/sec");
        Console.WriteLine("   • Latency: 1-5ms");
        Console.WriteLine("   • Features: Exchanges, routing, clustering");
        Console.WriteLine("   • Best for: High-throughput, complex routing");

        Console.WriteLine("\n☁️ Azure Service Bus:");
        Console.WriteLine("   • Throughput: 1K-10K messages/sec");
        Console.WriteLine("   • Latency: 10-50ms");
        Console.WriteLine("   • Features: Topics, sessions, dead letter, scheduling");
        Console.WriteLine("   • Best for: Enterprise integration, .NET ecosystems");

        Console.WriteLine("\n🐇 Apache Kafka:");
        Console.WriteLine("   • Throughput: 100K-1M+ messages/sec");
        Console.WriteLine("   • Latency: Sub-millisecond");
        Console.WriteLine("   • Features: Streaming, partitioning, replay");
        Console.WriteLine("   • Best for: Event streaming, big data");

        Console.WriteLine("\n💾 In-Memory (Redis/Memory):");
        Console.WriteLine("   • Throughput: 1M+ messages/sec");
        Console.WriteLine("   • Latency: Microseconds");
        Console.WriteLine("   • Features: Fast, simple");
        Console.WriteLine("   • Best for: Caching, real-time processing");

        Console.WriteLine("\n⚖️ Choosing the Right Tool:");
        Console.WriteLine("   📦 Simple queuing → RabbitMQ");
        Console.WriteLine("   🏢 Enterprise integration → Azure Service Bus");
        Console.WriteLine("   📊 Event streaming → Kafka");
        Console.WriteLine("   ⚡ Ultra-low latency → In-memory/Redis");
        Console.WriteLine("   🔄 Request/Reply → Any with correlation IDs");
        Console.WriteLine("   📡 Pub/Sub → RabbitMQ, Service Bus, or Kafka");

        Console.WriteLine();
    }

    private static async Task WaitForKeyPress(string message)
    {
        Console.WriteLine($"\n{message}");
        Console.ReadKey();
        Console.WriteLine();
        await Task.Delay(100);
    }
}
