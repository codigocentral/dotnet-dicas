using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Dica57.MessageQueues.Models;

namespace Dica57.MessageQueues.Services;

/// <summary>
/// Serviço simulado do Azure Service Bus (demonstração sem conexão real)
/// </summary>
public class AzureServiceBusService
{
    private readonly ILogger<AzureServiceBusService> _logger;
    private readonly InMemoryMessageBroker _fallbackBroker;
    private bool _isConnected = false;

    public AzureServiceBusService(ILogger<AzureServiceBusService> logger, InMemoryMessageBroker fallbackBroker)
    {
        _logger = logger;
        _fallbackBroker = fallbackBroker;
    }

    /// <summary>
    /// Simula conexão com Azure Service Bus
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        _logger.LogInformation("☁️ Tentando conectar ao Azure Service Bus...");
        
        try
        {
            // Simula tentativa de conexão que falhará sem connection string
            await Task.Delay(100);
            
            // Em produção seria:
            // _client = new ServiceBusClient(connectionString);
            // _sender = _client.CreateSender(queueName);
            // _processor = _client.CreateProcessor(queueName);
            
            throw new Exception("Azure Service Bus connection string não configurada (demonstração simulada)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("⚠️ Azure Service Bus não disponível: {Error}. Usando fallback in-memory.", ex.Message);
            _isConnected = false;
            return false;
        }
    }

    /// <summary>
    /// Demonstra padrões do Azure Service Bus usando fallback
    /// </summary>
    public async Task DemonstrateServiceBusPatterns()
    {
        Console.WriteLine("☁️ 3. Azure Service Bus Patterns (Simulado)");
        Console.WriteLine("--------------------------------------------");

        if (!_isConnected)
        {
            await ConnectAsync();
            Console.WriteLine("📝 Usando implementação simulada (in-memory) para demonstração\n");
        }

        await DemonstrateQueues();
        await DemonstrateTopicsAndSubscriptions();
        await DemonstrateDeadLetterQueue();
        await DemonstrateScheduledMessages();
        await DemonstrateSessions();
    }

    private async Task DemonstrateQueues()
    {
        Console.WriteLine("📫 Service Bus Queues");
        Console.WriteLine("--------------------");

        // Simula fila FIFO com processamento sequencial
        var processedOrders = new List<int>();

        _fallbackBroker.Subscribe<OrderMessage>("servicebus.orders", async order =>
        {
            Console.WriteLine($"📦 Processing order {order.OrderId} - Customer: {order.CustomerName}");
            await Task.Delay(100); // Simula processamento
            
            processedOrders.Add(order.OrderId);
            Console.WriteLine($"✅ Order {order.OrderId} completed. Total processed: {processedOrders.Count}");
        });

        // Envia pedidos
        for (int i = 1; i <= 4; i++)
        {
            var order = new OrderMessage
            {
                OrderId = i,
                CustomerName = $"Customer {i}",
                CustomerId = $"CUST{i:D3}",
                TotalAmount = Random.Shared.Next(100, 1000),
                Items = new List<OrderItem>
                {
                    new() { ProductId = i, ProductName = $"Product {i}", Quantity = 2, UnitPrice = 50.00m }
                }
            };

            await _fallbackBroker.PublishAsync("servicebus.orders", order);
            await Task.Delay(150);
        }

        await Task.Delay(1000);
        Console.WriteLine("✅ Service Bus Queues demonstration completed\n");
    }

    private async Task DemonstrateTopicsAndSubscriptions()
    {
        Console.WriteLine("📢 Topics and Subscriptions");
        Console.WriteLine("---------------------------");

        // Simula diferentes subscriptions com filtros
        
        // Subscription 1: Apenas pedidos grandes (> $500)
        _fallbackBroker.Subscribe<OrderMessage>("servicebus.orders.large", async order =>
        {
            if (order.TotalAmount > 500)
            {
                Console.WriteLine($"💰 Large Orders Subscription: Processing high-value order {order.OrderId} (${order.TotalAmount})");
                await Task.Delay(200); // Processamento especial para pedidos grandes
            }
        });

        // Subscription 2: Todos os pedidos para auditoria
        _fallbackBroker.Subscribe<OrderMessage>("servicebus.orders.audit", async order =>
        {
            Console.WriteLine($"📋 Audit Subscription: Logging order {order.OrderId} - ${order.TotalAmount}");
            await Task.Delay(50);
        });

        // Subscription 3: Apenas pedidos urgentes (simulados por ID par)
        _fallbackBroker.Subscribe<OrderMessage>("servicebus.orders.urgent", async order =>
        {
            if (order.OrderId % 2 == 0) // Simula filtro de urgência
            {
                Console.WriteLine($"🚀 Urgent Subscription: Fast-tracking order {order.OrderId}");
                await Task.Delay(30);
            }
        });

        // Envia pedidos que serão filtrados pelas subscriptions
        var orders = new[]
        {
            new OrderMessage { OrderId = 101, CustomerName = "Alice", TotalAmount = 750.00m }, // Large + Audit
            new OrderMessage { OrderId = 102, CustomerName = "Bob", TotalAmount = 250.00m },   // Audit + Urgent
            new OrderMessage { OrderId = 103, CustomerName = "Charlie", TotalAmount = 150.00m }, // Apenas Audit
            new OrderMessage { OrderId = 104, CustomerName = "Diana", TotalAmount = 1200.00m }  // Large + Audit + Urgent
        };

        foreach (var order in orders)
        {
            // Publica para todos os tópicos (simula filtros do Service Bus)
            await _fallbackBroker.PublishAsync("servicebus.orders.audit", order);
            
            if (order.TotalAmount > 500)
                await _fallbackBroker.PublishAsync("servicebus.orders.large", order);
            
            if (order.OrderId % 2 == 0)
                await _fallbackBroker.PublishAsync("servicebus.orders.urgent", order);

            await Task.Delay(200);
        }

        await Task.Delay(1000);
        Console.WriteLine("✅ Topics and Subscriptions demonstration completed\n");
    }

    private async Task DemonstrateDeadLetterQueue()
    {
        Console.WriteLine("💀 Dead Letter Queue");
        Console.WriteLine("-------------------");

        var failureCount = 0;
        var deadLetterMessages = new List<OrderMessage>();

        // Handler principal que falha ocasionalmente
        _fallbackBroker.Subscribe<OrderMessage>("servicebus.processing", async order =>
        {
            // Simula falha para alguns pedidos
            if (order.OrderId == 2 || order.OrderId == 4)
            {
                failureCount++;
                Console.WriteLine($"❌ Processing failed for order {order.OrderId} (attempt {failureCount})");
                
                if (failureCount >= 3) // Após 3 tentativas, vai para dead letter
                {
                    deadLetterMessages.Add(order);
                    Console.WriteLine($"💀 Order {order.OrderId} moved to Dead Letter Queue");
                    return;
                }
                
                throw new Exception($"Simulated processing failure for order {order.OrderId}");
            }

            Console.WriteLine($"✅ Order {order.OrderId} processed successfully");
            await Task.Delay(100);
        });

        // Dead Letter Queue handler
        _fallbackBroker.Subscribe<OrderMessage>("servicebus.deadletter", async order =>
        {
            Console.WriteLine($"🔍 Dead Letter Handler: Analyzing failed order {order.OrderId}");
            Console.WriteLine($"    - Customer: {order.CustomerName}");
            Console.WriteLine($"    - Amount: ${order.TotalAmount}");
            Console.WriteLine($"    - Failure reason: Processing timeout/error");
            await Task.Delay(50);
        });

        // Envia pedidos, alguns falharão
        for (int i = 1; i <= 5; i++)
        {
            var order = new OrderMessage
            {
                OrderId = i,
                CustomerName = $"Customer {i}",
                TotalAmount = Random.Shared.Next(100, 500)
            };

            await _fallbackBroker.PublishAsync("servicebus.processing", order);
            await Task.Delay(200);
        }

        await Task.Delay(1000);

        // Processa mensagens em dead letter
        foreach (var deadLetter in deadLetterMessages)
        {
            await _fallbackBroker.PublishAsync("servicebus.deadletter", deadLetter);
        }

        await Task.Delay(500);
        Console.WriteLine($"✅ Dead Letter Queue demonstration completed. {deadLetterMessages.Count} messages in DLQ\n");
    }

    private async Task DemonstrateScheduledMessages()
    {
        Console.WriteLine("⏰ Scheduled Messages");
        Console.WriteLine("--------------------");

        _fallbackBroker.Subscribe<NotificationMessage>("servicebus.scheduled", async notification =>
        {
            Console.WriteLine($"⏰ Scheduled notification delivered: {notification.Subject} to {notification.To}");
            await Task.Delay(50);
        });

        // Simula mensagens agendadas (em produção usaria ScheduledEnqueueTimeUtc)
        var scheduledNotifications = new[]
        {
            new { delay = TimeSpan.FromSeconds(1), message = new NotificationMessage { To = "user1@test.com", Subject = "Lembrete: Reunião em 1 hora" } },
            new { delay = TimeSpan.FromSeconds(2), message = new NotificationMessage { To = "user2@test.com", Subject = "Lembrete: Pagamento pendente" } },
            new { delay = TimeSpan.FromSeconds(3), message = new NotificationMessage { To = "user3@test.com", Subject = "Lembrete: Avaliação do produto" } }
        };

        Console.WriteLine("📅 Agendando mensagens para entrega futura...");
        
        foreach (var scheduled in scheduledNotifications)
        {
            // Simula agendamento
            _ = Task.Run(async () =>
            {
                await Task.Delay(scheduled.delay);
                await _fallbackBroker.PublishAsync("servicebus.scheduled", scheduled.message);
            });
        }

        await Task.Delay(4000); // Aguarda todas as mensagens serem entregues
        Console.WriteLine("✅ Scheduled Messages demonstration completed\n");
    }

    private async Task DemonstrateSessions()
    {
        Console.WriteLine("🎭 Message Sessions");
        Console.WriteLine("------------------");

        var sessionMessages = new Dictionary<string, List<OrderMessage>>();

        _fallbackBroker.Subscribe<OrderMessage>("servicebus.sessions", async order =>
        {
            var sessionId = order.CustomerId; // Agrupa por cliente
            
            if (!sessionMessages.ContainsKey(sessionId))
            {
                sessionMessages[sessionId] = new List<OrderMessage>();
                Console.WriteLine($"🎭 New session started for customer: {sessionId}");
            }
            
            sessionMessages[sessionId].Add(order);
            
            Console.WriteLine($"📦 Session {sessionId}: Processing order {order.OrderId} (order #{sessionMessages[sessionId].Count} in session)");
            
            // Simula processamento sequencial por sessão
            await Task.Delay(100);
            
            Console.WriteLine($"✅ Session {sessionId}: Order {order.OrderId} completed");
        });

        // Envia pedidos de diferentes clientes (sessões)
        var sessionOrders = new[]
        {
            new OrderMessage { OrderId = 1, CustomerId = "CUST001", CustomerName = "Alice", TotalAmount = 100 },
            new OrderMessage { OrderId = 2, CustomerId = "CUST002", CustomerName = "Bob", TotalAmount = 200 },
            new OrderMessage { OrderId = 3, CustomerId = "CUST001", CustomerName = "Alice", TotalAmount = 300 }, // Mesma sessão
            new OrderMessage { OrderId = 4, CustomerId = "CUST003", CustomerName = "Charlie", TotalAmount = 400 },
            new OrderMessage { OrderId = 5, CustomerId = "CUST002", CustomerName = "Bob", TotalAmount = 500 }, // Mesma sessão
            new OrderMessage { OrderId = 6, CustomerId = "CUST001", CustomerName = "Alice", TotalAmount = 600 }  // Mesma sessão
        };

        foreach (var order in sessionOrders)
        {
            await _fallbackBroker.PublishAsync("servicebus.sessions", order);
            await Task.Delay(150);
        }

        await Task.Delay(1000);
        
        Console.WriteLine("📊 Session Summary:");
        foreach (var session in sessionMessages)
        {
            Console.WriteLine($"   Session {session.Key}: {session.Value.Count} orders processed");
        }
        
        Console.WriteLine("✅ Message Sessions demonstration completed\n");
    }

    public void Dispose()
    {
        _logger.LogInformation("☁️ Desconectando do Azure Service Bus...");
        // Em produção: await _processor.StopProcessingAsync(); await _client.DisposeAsync();
    }
}
