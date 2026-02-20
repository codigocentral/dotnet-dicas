using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace Dica56_gRPC.Services;

/// <summary>
/// Serviço gRPC para demonstrar diferentes tipos de comunicação
/// </summary>
public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private static int _callCounter = 0;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Chamada Unária - Simples request/response
    /// </summary>
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var count = Interlocked.Increment(ref _callCounter);
        
        _logger.LogInformation("🔄 Recebida chamada unária de: {Name} (Chamada #{Count})", 
            request.Name, count);

        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name}! Esta é uma chamada gRPC unária.",
            Count = count,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });
    }

    /// <summary>
    /// Server Streaming - O servidor envia múltiplas respostas
    /// </summary>
    public override async Task SayHelloServerStreaming(HelloRequest request, 
        IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("📡 Iniciando server streaming para: {Name}", request.Name);

        try
        {
            for (int i = 1; i <= 10; i++)
            {
                // Verificar se o cliente cancelou a operação
                context.CancellationToken.ThrowIfCancellationRequested();

                var reply = new HelloReply
                {
                    Message = $"Streaming message {i}/10 para {request.Name}",
                    Count = i,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")
                };

                await responseStream.WriteAsync(reply);
                
                _logger.LogInformation("📤 Enviada mensagem {Number}/10 para {Name}", i, request.Name);

                // Simular delay entre mensagens
                await Task.Delay(1000, context.CancellationToken);
            }

            _logger.LogInformation("✅ Server streaming completo para: {Name}", request.Name);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Server streaming cancelado pelo cliente: {Name}", request.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no server streaming para: {Name}", request.Name);
            throw;
        }
    }

    /// <summary>
    /// Client Streaming - O cliente envia múltiplas requisições
    /// </summary>
    public override async Task<HelloReply> SayHelloClientStreaming(
        IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        _logger.LogInformation("📨 Iniciando client streaming");

        var names = new List<string>();
        var messageCount = 0;

        try
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                messageCount++;
                names.Add(request.Name);
                
                _logger.LogInformation("📥 Recebida mensagem {Count}: {Name}", 
                    messageCount, request.Name);
            }

            var combinedNames = string.Join(", ", names.Distinct());
            var response = new HelloReply
            {
                Message = $"Recebidas {messageCount} mensagens de: {combinedNames}",
                Count = messageCount,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")
            };

            _logger.LogInformation("✅ Client streaming completo. Total: {Count} mensagens", messageCount);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no client streaming");
            throw;
        }
    }

    /// <summary>
    /// Bidirectional Streaming - Cliente e servidor enviam múltiplas mensagens
    /// </summary>
    public override async Task SayHelloBidirectional(
        IAsyncStreamReader<HelloRequest> requestStream,
        IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        _logger.LogInformation("🔄 Iniciando bidirectional streaming");

        try
        {
            var messageCount = 0;

            await foreach (var request in requestStream.ReadAllAsync())
            {
                messageCount++;
                
                _logger.LogInformation("📨 Recebida mensagem bidirectional: {Name} #{Count}", 
                    request.Name, messageCount);

                // Responder imediatamente a cada mensagem recebida
                var reply = new HelloReply
                {
                    Message = $"Echo para {request.Name}: Mensagem #{messageCount} processada",
                    Count = messageCount,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")
                };

                await responseStream.WriteAsync(reply);
                
                _logger.LogInformation("📤 Enviada resposta bidirectional para: {Name} #{Count}", 
                    request.Name, messageCount);

                // Simular algum processamento
                await Task.Delay(500, context.CancellationToken);
            }

            _logger.LogInformation("✅ Bidirectional streaming completo. Total: {Count} mensagens", 
                messageCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Bidirectional streaming cancelado");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no bidirectional streaming");
            throw;
        }
    }
}
