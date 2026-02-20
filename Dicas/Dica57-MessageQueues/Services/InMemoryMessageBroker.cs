using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Dica57.MessageQueues.Models;

namespace Dica57.MessageQueues.Services;

/// <summary>
/// Message Broker em memória para demonstrações básicas
/// </summary>
public class InMemoryMessageBroker
{
    private readonly ILogger<InMemoryMessageBroker> _logger;
    private readonly ConcurrentDictionary<string, List<Func<BaseMessage, Task>>> _subscribers = new();
    private readonly ConcurrentQueue<(string Topic, BaseMessage Message)> _messageQueue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly QueueStatistics _statistics = new() { QueueName = "InMemory" };

    public InMemoryMessageBroker(ILogger<InMemoryMessageBroker> logger)
    {
        _logger = logger;
        StartMessageProcessor();
    }

    /// <summary>
    /// Publica uma mensagem em um tópico
    /// </summary>
    public async Task PublishAsync<T>(string topic, T message) where T : BaseMessage
    {
        message.MessageType = typeof(T).Name;
        message.Timestamp = DateTime.UtcNow;

        _messageQueue.Enqueue((topic, message));
        _statistics.MessagesProduced++;
        _statistics.LastMessageTime = DateTime.UtcNow;

        _logger.LogInformation("📤 Mensagem publicada: {Topic} - {MessageId} ({MessageType})",
            topic, message.Id, message.MessageType);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Subscreve a um tópico com um handler
    /// </summary>
    public void Subscribe<T>(string topic, Func<T, Task> handler) where T : BaseMessage
    {
        var wrappedHandler = new Func<BaseMessage, Task>(async message =>
        {
            if (message is T typedMessage)
            {
                await handler(typedMessage);
            }
        });

        _subscribers.AddOrUpdate(topic,
            [wrappedHandler],
            (key, existing) =>
            {
                existing.Add(wrappedHandler);
                return existing;
            });

        _logger.LogInformation("📥 Novo subscriber para tópico: {Topic} ({MessageType})",
            topic, typeof(T).Name);
    }

    /// <summary>
    /// Obtém estatísticas da fila
    /// </summary>
    public QueueStatistics GetStatistics()
    {
        _statistics.MessagesInQueue = _messageQueue.Count;
        return _statistics;
    }

    private void StartMessageProcessor()
    {
        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var item))
                {
                    await ProcessMessage(item.Topic, item.Message);
                }
                else
                {
                    await Task.Delay(10, _cancellationTokenSource.Token);
                }
            }
        }, _cancellationTokenSource.Token);
    }

    private async Task ProcessMessage(string topic, BaseMessage message)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            if (_subscribers.TryGetValue(topic, out var handlers))
            {
                var tasks = handlers.Select(handler => ExecuteHandlerSafely(handler, message));
                await Task.WhenAll(tasks);

                _statistics.MessagesConsumed++;
                var processingTime = DateTime.UtcNow - startTime;
                
                _logger.LogInformation("✅ Mensagem processada: {MessageId} em {ProcessingTime}ms",
                    message.Id, processingTime.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar mensagem {MessageId}", message.Id);
        }
    }

    private async Task ExecuteHandlerSafely(Func<BaseMessage, Task> handler, BaseMessage message)
    {
        try
        {
            await handler(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no handler para mensagem {MessageId}", message.Id);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
