using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Dica57.MessageQueues.Models;

namespace Dica57.MessageQueues.Services;

/// <summary>
/// Serviço simulado do RabbitMQ (demonstração sem servidor real)
/// </summary>
public class RabbitMQService
{
    private readonly ILogger<RabbitMQService> _logger;
    private readonly InMemoryMessageBroker _fallbackBroker;
    private bool _isConnected = false;

    public RabbitMQService(ILogger<RabbitMQService> logger, InMemoryMessageBroker fallbackBroker)
    {
        _logger = logger;
        _fallbackBroker = fallbackBroker;
    }

    /// <summary>
    /// Simula conexão com RabbitMQ
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        _logger.LogInformation("🐰 Tentando conectar ao RabbitMQ...");
        
        try
        {
            // Simula tentativa de conexão que falhará sem servidor
            await Task.Delay(100);
            
            // Em produção seria:
            // var factory = new ConnectionFactory() { HostName = "localhost" };
            // _connection = factory.CreateConnection();
            // _channel = _connection.CreateModel();
            
            throw new Exception("RabbitMQ server não disponível (demonstração simulada)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("⚠️ RabbitMQ não disponível: {Error}. Usando fallback in-memory.", ex.Message);
            _isConnected = false;
            return false;
        }
    }

    /// <summary>
    /// Demonstra padrões do RabbitMQ usando fallback
    /// </summary>
    public async Task DemonstrateRabbitMQPatterns()
    {
        Console.WriteLine("🐰 2. RabbitMQ Patterns (Simulado)");
        Console.WriteLine("----------------------------------");

        if (!_isConnected)
        {
            await ConnectAsync();
            Console.WriteLine("📝 Usando implementação simulada (in-memory) para demonstração\n");
        }

        await DemonstrateWorkQueues();
        await DemonstratePublishSubscribe();
        await DemonstrateRouting();
        await DemonstrateTopics();
        await DemonstrateRPC();
    }

    private async Task DemonstrateWorkQueues()
    {
        Console.WriteLine("👷 Work Queues (Task Distribution)");
        Console.WriteLine("-----------------------------------");

        // Simula work queue com múltiplos workers
        var workersStarted = 0;

        // Worker 1
        _fallbackBroker.Subscribe<OrderMessage>("work.orders", async order =>
        {
            var workerId = Interlocked.Increment(ref workersStarted) % 3 + 1;
            Console.WriteLine($"👷‍♂️ Worker {workerId}: Processando pedido {order.OrderId}");
            await Task.Delay(Random.Shared.Next(100, 500)); // Simula trabalho
            Console.WriteLine($"✅ Worker {workerId}: Pedido {order.OrderId} concluído");
        });

        // Worker 2
        _fallbackBroker.Subscribe<OrderMessage>("work.orders", async order =>
        {
            var workerId = Interlocked.Increment(ref workersStarted) % 3 + 1;
            Console.WriteLine($"👷‍♀️ Worker {workerId}: Processando pedido {order.OrderId}");
            await Task.Delay(Random.Shared.Next(100, 500));
            Console.WriteLine($"✅ Worker {workerId}: Pedido {order.OrderId} concluído");
        });

        // Envia trabalhos
        for (int i = 1; i <= 5; i++)
        {
            var order = new OrderMessage
            {
                OrderId = i,
                CustomerName = $"Cliente {i}",
                TotalAmount = Random.Shared.Next(50, 500)
            };

            await _fallbackBroker.PublishAsync("work.orders", order);
            await Task.Delay(200);
        }

        await Task.Delay(2000);
        Console.WriteLine("✅ Work Queue demonstration completed\n");
    }

    private async Task DemonstratePublishSubscribe()
    {
        Console.WriteLine("📡 Publish/Subscribe (Fanout Exchange)");
        Console.WriteLine("--------------------------------------");

        // Múltiplos subscribers para o mesmo evento
        _fallbackBroker.Subscribe<UserEventMessage>("user.events", async userEvent =>
        {
            Console.WriteLine($"📧 Email Service: Enviando email para {userEvent.Email} - {userEvent.EventType}");
            await Task.Delay(50);
        });

        _fallbackBroker.Subscribe<UserEventMessage>("user.events", async userEvent =>
        {
            Console.WriteLine($"📊 Analytics Service: Registrando evento {userEvent.EventType} para usuário {userEvent.UserId}");
            await Task.Delay(30);
        });

        _fallbackBroker.Subscribe<UserEventMessage>("user.events", async userEvent =>
        {
            Console.WriteLine($"🔔 Notification Service: Push notification para {userEvent.UserName}");
            await Task.Delay(40);
        });

        // Publica eventos
        var events = new[]
        {
            new UserEventMessage { UserId = 1, UserName = "João", Email = "joao@test.com", EventType = "Created" },
            new UserEventMessage { UserId = 2, UserName = "Maria", Email = "maria@test.com", EventType = "LoggedIn" },
            new UserEventMessage { UserId = 1, UserName = "João", Email = "joao@test.com", EventType = "Updated" }
        };

        foreach (var evt in events)
        {
            await _fallbackBroker.PublishAsync("user.events", evt);
            await Task.Delay(300);
        }

        await Task.Delay(1000);
        Console.WriteLine("✅ Publish/Subscribe demonstration completed\n");
    }

    private async Task DemonstrateRouting()
    {
        Console.WriteLine("🎯 Routing (Direct Exchange)");
        Console.WriteLine("-----------------------------");

        // Handlers para diferentes prioridades
        _fallbackBroker.Subscribe<NotificationMessage>("notifications.high", async notification =>
        {
            Console.WriteLine($"🚨 HIGH Priority Handler: {notification.Subject} para {notification.To}");
            await Task.Delay(50);
        });

        _fallbackBroker.Subscribe<NotificationMessage>("notifications.normal", async notification =>
        {
            Console.WriteLine($"📝 NORMAL Priority Handler: {notification.Subject} para {notification.To}");
            await Task.Delay(100);
        });

        _fallbackBroker.Subscribe<NotificationMessage>("notifications.low", async notification =>
        {
            Console.WriteLine($"📄 LOW Priority Handler: {notification.Subject} para {notification.To}");
            await Task.Delay(150);
        });

        // Envia notificações com diferentes prioridades
        var notifications = new[]
        {
            new NotificationMessage { To = "admin@test.com", Subject = "Sistema indisponível", Priority = NotificationPriority.Critical },
            new NotificationMessage { To = "user@test.com", Subject = "Pedido confirmado", Priority = NotificationPriority.Normal },
            new NotificationMessage { To = "marketing@test.com", Subject = "Newsletter semanal", Priority = NotificationPriority.Low }
        };

        foreach (var notification in notifications)
        {
            var topic = notification.Priority switch
            {
                NotificationPriority.Critical or NotificationPriority.High => "notifications.high",
                NotificationPriority.Normal => "notifications.normal",
                _ => "notifications.low"
            };

            await _fallbackBroker.PublishAsync(topic, notification);
            await Task.Delay(200);
        }

        await Task.Delay(1000);
        Console.WriteLine("✅ Routing demonstration completed\n");
    }

    private async Task DemonstrateTopics()
    {
        Console.WriteLine("🏷️ Topics (Topic Exchange)");
        Console.WriteLine("---------------------------");

        // Handlers para diferentes patterns de tópicos
        _fallbackBroker.Subscribe<SystemMetricMessage>("metrics.cpu", async metric =>
        {
            Console.WriteLine($"🖥️ CPU Monitor: {metric.ServiceName} - {metric.MetricName}: {metric.Value}{metric.Unit}");
            await Task.Delay(30);
        });

        _fallbackBroker.Subscribe<SystemMetricMessage>("metrics.memory", async metric =>
        {
            Console.WriteLine($"💾 Memory Monitor: {metric.ServiceName} - {metric.MetricName}: {metric.Value}{metric.Unit}");
            await Task.Delay(30);
        });

        _fallbackBroker.Subscribe<SystemMetricMessage>("metrics.all", async metric =>
        {
            Console.WriteLine($"📊 All Metrics Collector: {metric.ServiceName}.{metric.MetricName} = {metric.Value}");
            await Task.Delay(20);
        });

        // Envia métricas
        var metrics = new[]
        {
            new SystemMetricMessage { ServiceName = "web-api", MetricName = "cpu_usage", Value = 85.5, Unit = "%" },
            new SystemMetricMessage { ServiceName = "web-api", MetricName = "memory_usage", Value = 2.1, Unit = "GB" },
            new SystemMetricMessage { ServiceName = "database", MetricName = "cpu_usage", Value = 45.2, Unit = "%" },
            new SystemMetricMessage { ServiceName = "database", MetricName = "memory_usage", Value = 8.7, Unit = "GB" }
        };

        foreach (var metric in metrics)
        {
            var specificTopic = metric.MetricName.Contains("cpu") ? "metrics.cpu" : "metrics.memory";
            
            await _fallbackBroker.PublishAsync(specificTopic, metric);
            await _fallbackBroker.PublishAsync("metrics.all", metric);
            await Task.Delay(150);
        }

        await Task.Delay(1000);
        Console.WriteLine("✅ Topics demonstration completed\n");
    }

    private async Task DemonstrateRPC()
    {
        Console.WriteLine("🔄 RPC (Request/Reply)");
        Console.WriteLine("----------------------");

        var responseReceived = false;
        var correlationId = Guid.NewGuid().ToString();

        // RPC Server (responde a requests)
        _fallbackBroker.Subscribe<OrderMessage>("rpc.calculate_shipping", async order =>
        {
            Console.WriteLine($"🧮 RPC Server: Calculando frete para pedido {order.OrderId}");
            await Task.Delay(200); // Simula cálculo

            var shippingCost = order.TotalAmount * 0.1m; // 10% do valor
            var response = new PaymentResponseMessage
            {
                OrderId = order.OrderId,
                Amount = shippingCost,
                Currency = "USD",
                Status = PaymentStatus.Completed,
                CorrelationId = order.CorrelationId
            };

            // Responde no tópico de reply
            if (!string.IsNullOrEmpty(order.ReplyTo))
            {
                await _fallbackBroker.PublishAsync(order.ReplyTo, response);
            }
        });

        // RPC Client (aguarda resposta)
        _fallbackBroker.Subscribe<PaymentResponseMessage>($"rpc.reply.{correlationId}", async response =>
        {
            Console.WriteLine($"✅ RPC Client: Frete calculado: ${response.Amount:F2} para pedido {response.OrderId}");
            responseReceived = true;
            await Task.CompletedTask;
        });

        // Envia RPC request
        var rpcOrder = new OrderMessage
        {
            OrderId = 999,
            CustomerName = "Cliente RPC",
            TotalAmount = 150.00m,
            CorrelationId = correlationId,
            ReplyTo = $"rpc.reply.{correlationId}"
        };

        Console.WriteLine("📤 RPC Client: Enviando request para cálculo de frete...");
        await _fallbackBroker.PublishAsync("rpc.calculate_shipping", rpcOrder);

        // Aguarda resposta
        var timeout = DateTime.UtcNow.AddSeconds(3);
        while (!responseReceived && DateTime.UtcNow < timeout)
        {
            await Task.Delay(50);
        }

        Console.WriteLine(responseReceived ? "✅ RPC completed successfully" : "⏰ RPC timeout");
        Console.WriteLine();
    }

    public void Dispose()
    {
        _logger.LogInformation("🐰 Desconectando do RabbitMQ...");
        // Em produção: _channel?.Close(); _connection?.Close();
    }
}
