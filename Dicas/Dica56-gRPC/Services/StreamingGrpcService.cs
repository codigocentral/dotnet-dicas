using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace Dica56_gRPC.Services;

/// <summary>
/// Serviço gRPC para demonstrar funcionalidades de streaming
/// </summary>
public class StreamingGrpcService : StreamingService.StreamingServiceBase
{
    private readonly ILogger<StreamingGrpcService> _logger;
    private readonly Random _random;

    public StreamingGrpcService(ILogger<StreamingGrpcService> logger)
    {
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// Stream de dados em tempo real
    /// </summary>
    public override async Task StreamData(StreamDataRequest request, 
        IServerStreamWriter<DataPoint> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("📡 Iniciando stream de dados: Tipo={Type}, Intervalo={Interval}s, Duração={Duration}s", 
            request.DataType, request.IntervalSeconds, request.DurationSeconds);

        try
        {
            var endTime = DateTime.UtcNow.AddSeconds(request.DurationSeconds);
            var dataPointId = 1;

            while (DateTime.UtcNow < endTime && !context.CancellationToken.IsCancellationRequested)
            {
                var dataPoint = GenerateDataPoint(request.DataType, dataPointId++);
                
                await responseStream.WriteAsync(dataPoint);
                
                _logger.LogDebug("📤 Enviado data point: {Name}={Value} {Unit}", 
                    dataPoint.Name, dataPoint.Value, dataPoint.Unit);

                await Task.Delay(request.IntervalSeconds * 1000, context.CancellationToken);
            }

            _logger.LogInformation("✅ Stream de dados finalizado: {Type}", request.DataType);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Stream de dados cancelado: {Type}", request.DataType);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no stream de dados: {Type}", request.DataType);
            throw;
        }
    }

    /// <summary>
    /// Upload de arquivo em chunks
    /// </summary>
    public override async Task<UploadResponse> UploadFile(IAsyncStreamReader<FileChunk> requestStream, 
        ServerCallContext context)
    {
        _logger.LogInformation("📁 Iniciando upload de arquivo");

        try
        {
            string fileName = "";
            var fileData = new List<byte>();
            var chunkCount = 0;

            await foreach (var chunk in requestStream.ReadAllAsync())
            {
                chunkCount++;
                
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = chunk.FileName;
                    _logger.LogInformation("📂 Arquivo: {FileName}", fileName);
                }

                fileData.AddRange(chunk.Data.ToByteArray());
                
                _logger.LogDebug("📦 Recebido chunk {Number} - {Size} bytes", 
                    chunk.ChunkNumber, chunk.Data.Length);

                if (chunk.IsLastChunk)
                {
                    _logger.LogInformation("✅ Último chunk recebido");
                    break;
                }
            }

            // Simular salvamento do arquivo
            var fileId = Guid.NewGuid().ToString();
            var totalSize = fileData.Count;

            _logger.LogInformation("💾 Arquivo salvo: {FileName} - {Size} bytes - {Chunks} chunks", 
                fileName, totalSize, chunkCount);

            return new UploadResponse
            {
                FileId = fileId,
                FileName = fileName,
                FileSize = totalSize,
                Status = "SUCCESS",
                Message = $"Arquivo {fileName} enviado com sucesso ({chunkCount} chunks, {totalSize} bytes)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no upload de arquivo");
            throw new RpcException(new Status(StatusCode.Internal, 
                "Erro interno no upload de arquivo"));
        }
    }

    /// <summary>
    /// Chat bidirectional em tempo real
    /// </summary>
    public override async Task Chat(IAsyncStreamReader<ChatMessage> requestStream,
        IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("💬 Iniciando chat bidirectional");

        try
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                _logger.LogInformation("📨 Mensagem do chat: {User} na sala {Room}: {Message}", 
                    message.UserName, message.RoomId, message.Message);

                // Eco da mensagem com timestamp do servidor
                var echoMessage = new ChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = "SERVER",
                    UserName = "Echo Bot",
                    Message = $"Echo: {message.Message}",
                    RoomId = message.RoomId,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    Type = ChatMessageType.System
                };

                await responseStream.WriteAsync(echoMessage);
                
                _logger.LogInformation("📤 Echo enviado para sala {Room}", message.RoomId);
            }

            _logger.LogInformation("✅ Chat finalizado");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Chat cancelado");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no chat");
            throw;
        }
    }

    /// <summary>
    /// Monitoramento de métricas em tempo real
    /// </summary>
    public override async Task MonitorMetrics(MetricsRequest request,
        IServerStreamWriter<MetricUpdate> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("📊 Iniciando monitoramento de métricas: {Metrics}", 
            string.Join(", ", request.MetricNames));

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                foreach (var metricName in request.MetricNames)
                {
                    var metricUpdate = GenerateMetricUpdate(metricName);
                    
                    await responseStream.WriteAsync(metricUpdate);
                    
                    _logger.LogDebug("📈 Métrica enviada: {Name}={Value} {Unit}", 
                        metricUpdate.MetricName, metricUpdate.Value, metricUpdate.Unit);
                }

                await Task.Delay(request.IntervalSeconds * 1000, context.CancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Monitoramento de métricas cancelado");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no monitoramento de métricas");
            throw;
        }
    }

    /// <summary>
    /// Gera um ponto de dados simulado
    /// </summary>
    private DataPoint GenerateDataPoint(string dataType, int id)
    {
        var dataPoint = new DataPoint
        {
            Id = id.ToString(),
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        switch (dataType.ToLower())
        {
            case "temperature":
                dataPoint.Name = "Temperature";
                dataPoint.Value = 20 + _random.NextDouble() * 15; // 20-35°C
                dataPoint.Unit = "°C";
                dataPoint.Metadata.Add("sensor", "DHT22");
                dataPoint.Metadata.Add("location", "Server Room");
                break;

            case "cpu":
                dataPoint.Name = "CPU Usage";
                dataPoint.Value = _random.NextDouble() * 100; // 0-100%
                dataPoint.Unit = "%";
                dataPoint.Metadata.Add("core", "average");
                dataPoint.Metadata.Add("server", "web-01");
                break;

            case "memory":
                dataPoint.Name = "Memory Usage";
                dataPoint.Value = 2 + _random.NextDouble() * 14; // 2-16 GB
                dataPoint.Unit = "GB";
                dataPoint.Metadata.Add("type", "physical");
                dataPoint.Metadata.Add("server", "web-01");
                break;

            default:
                dataPoint.Name = "Random Value";
                dataPoint.Value = _random.NextDouble() * 1000;
                dataPoint.Unit = "units";
                break;
        }

        return dataPoint;
    }

    /// <summary>
    /// Gera uma atualização de métrica simulada
    /// </summary>
    private MetricUpdate GenerateMetricUpdate(string metricName)
    {
        var update = new MetricUpdate
        {
            MetricName = metricName,
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        switch (metricName.ToLower())
        {
            case "requests_per_second":
                update.Value = 100 + _random.NextDouble() * 900; // 100-1000 RPS
                update.Unit = "req/s";
                update.Labels.Add("service", "api");
                update.Labels.Add("endpoint", "/products");
                break;

            case "error_rate":
                update.Value = _random.NextDouble() * 5; // 0-5%
                update.Unit = "%";
                update.Labels.Add("service", "api");
                update.Labels.Add("type", "5xx");
                break;

            case "response_time":
                update.Value = 50 + _random.NextDouble() * 200; // 50-250ms
                update.Unit = "ms";
                update.Labels.Add("service", "api");
                update.Labels.Add("percentile", "p95");
                break;

            default:
                update.Value = _random.NextDouble() * 100;
                update.Unit = "units";
                break;
        }

        return update;
    }
}
