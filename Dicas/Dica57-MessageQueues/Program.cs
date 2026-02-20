using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Dica57.MessageQueues.Services;
using Dica57.MessageQueues.Models;

Console.WriteLine("🎯 Dica 57: Message Queues - RabbitMQ e Azure Service Bus");
Console.WriteLine("===========================================================\n");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        // Registrar serviços de Message Queue
        services.AddSingleton<InMemoryMessageBroker>();
        services.AddSingleton<RabbitMQService>();
        services.AddSingleton<AzureServiceBusService>();
        services.AddSingleton<MessageQueueDemoService>();
    })
    .Build();

var demoService = host.Services.GetRequiredService<MessageQueueDemoService>();

try
{
    await demoService.RunAllDemonstrations();
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Erro durante a demonstração");
    Console.WriteLine($"❌ Erro: {ex.Message}");
}

Console.WriteLine("\n🎉 Demonstração Completa!");
Console.WriteLine("=========================");
Console.WriteLine("✅ In-Memory Message Broker - Pub/Sub simples");
Console.WriteLine("✅ RabbitMQ Patterns - Exchanges, Queues, Routing");
Console.WriteLine("✅ Azure Service Bus - Topics, Subscriptions, Dead Letter");
Console.WriteLine("✅ Message Patterns - Fire-and-Forget, Request/Reply, Publish/Subscribe");
Console.WriteLine("✅ Error Handling - Dead Letter Queues, Retry Policies");
Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();
