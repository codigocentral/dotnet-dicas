using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("🌐 Dica 94: Advanced Networking & Protocols (.NET 9)");
Console.WriteLine("====================================================");

// 1. TCP Socket Programming
Console.WriteLine("\n1. 🔌 TCP Socket Programming:");
Console.WriteLine("─────────────────────────────");

await DemonstrarTcpSockets();

// 2. UDP Socket Programming
Console.WriteLine("\n2. 📡 UDP Socket Programming:");
Console.WriteLine("─────────────────────────────");

await DemonstrarUdpSockets();

// 3. WebSocket Client
Console.WriteLine("\n3. 🔗 WebSocket Client:");
Console.WriteLine("──────────────────────");

await DemonstrarWebSocketClient();

// 4. HTTP/2 Client Features
Console.WriteLine("\n4. 🚀 HTTP/2 Advanced Features:");
Console.WriteLine("───────────────────────────────");

await DemonstrarHttp2Features();

// 5. Custom Network Protocol
Console.WriteLine("\n5. 🛠️ Custom Network Protocol:");
Console.WriteLine("──────────────────────────────");

await DemonstrarProtocoloCustomizado();

// 6. Network Diagnostics & Monitoring
Console.WriteLine("\n6. 📊 Network Diagnostics:");
Console.WriteLine("──────────────────────────");

await DemonstrarDiagnosticosRede();

// 7. SignalR Client Simulation
Console.WriteLine("\n7. 📱 SignalR Client Advanced:");
Console.WriteLine("─────────────────────────────");

await DemonstrarSignalRAvancado();

// 8. Network Performance & Optimization
Console.WriteLine("\n8. ⚡ Network Performance:");
Console.WriteLine("─────────────────────────────");

await DemonstrarPerformanceRede();

Console.WriteLine("\n✅ Demonstração completa de Advanced Networking!");

static async Task DemonstrarTcpSockets()
{
    // TCP Server simulation (listener)
    var tcpListener = new TcpListener(IPAddress.Loopback, 8080);
    tcpListener.Start();
    
    Console.WriteLine("🖥️ TCP Server iniciado em 127.0.0.1:8080");
    
    // Simulate client connection in background
    _ = Task.Run(async () =>
    {
        await Task.Delay(100); // Wait for listener to be ready
        
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, 8080);
            
            var stream = client.GetStream();
            var message = Encoding.UTF8.GetBytes("Hello TCP Server!");
            
            await stream.WriteAsync(message);
            Console.WriteLine($"📤 Cliente enviou: {Encoding.UTF8.GetString(message)}");
            
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"📥 Cliente recebeu: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro cliente TCP: {ex.GetType().Name}");
        }
    });
    
    // Accept one connection
    try
    {
        using var tcpClient = await tcpListener.AcceptTcpClientAsync();
        Console.WriteLine($"✅ Cliente conectado: {tcpClient.Client.RemoteEndPoint}");
        
        var stream = tcpClient.GetStream();
        var buffer = new byte[1024];
        var bytesRead = await stream.ReadAsync(buffer);
        
        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"📥 Servidor recebeu: {receivedMessage}");
        
        // Echo response
        var response = Encoding.UTF8.GetBytes($"Echo: {receivedMessage}");
        await stream.WriteAsync(response);
        Console.WriteLine($"📤 Servidor respondeu: {Encoding.UTF8.GetString(response)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro servidor TCP: {ex.GetType().Name}");
    }
    finally
    {
        tcpListener.Stop();
        Console.WriteLine("🛑 TCP Server parado");
    }
    
    await Task.Delay(100);
}

static async Task DemonstrarUdpSockets()
{
    // UDP Server
    using var udpServer = new UdpClient(8081);
    Console.WriteLine("📡 UDP Server iniciado na porta 8081");
    
    // UDP Client simulation
    _ = Task.Run(async () =>
    {
        await Task.Delay(100);
        
        try
        {
            using var udpClient = new UdpClient();
            var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 8081);
            
            var messages = new[]
            {
                "UDP Message 1",
                "UDP Message 2", 
                "UDP Message 3"
            };
            
            foreach (var msg in messages)
            {
                var data = Encoding.UTF8.GetBytes(msg);
                await udpClient.SendAsync(data, serverEndpoint);
                Console.WriteLine($"📤 UDP Cliente enviou: {msg}");
                await Task.Delay(50);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro cliente UDP: {ex.GetType().Name}");
        }
    });
    
    // Receive messages
    try
    {
        for (int i = 0; i < 3; i++)
        {
            var result = await udpServer.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"📥 UDP Servidor recebeu: {message} de {result.RemoteEndPoint}");
            
            // Send response
            var response = Encoding.UTF8.GetBytes($"ACK: {message}");
            await udpServer.SendAsync(response, result.RemoteEndPoint);
            Console.WriteLine($"📤 UDP Servidor respondeu: ACK");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro servidor UDP: {ex.GetType().Name}");
    }
    
    Console.WriteLine("🛑 UDP Server finalizado");
    await Task.Delay(100);
}

static async Task DemonstrarWebSocketClient()
{
    // Simulate WebSocket server endpoint (echo.websocket.org for demo)
    var uri = new Uri("wss://echo.websocket.org");
    
    try
    {
        using var webSocket = new ClientWebSocket();
        
        // Add headers
        webSocket.Options.SetRequestHeader("User-Agent", "DemoApp/1.0");
        
        Console.WriteLine($"🔗 Conectando WebSocket: {uri}");
        await webSocket.ConnectAsync(uri, CancellationToken.None);
        Console.WriteLine($"✅ WebSocket conectado - Estado: {webSocket.State}");
        
        // Send messages
        var messages = new[]
        {
            "Hello WebSocket!",
            "This is a test message",
            "WebSocket communication works!"
        };
        
        foreach (var message in messages)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
            
            Console.WriteLine($"📤 Enviado: {message}");
            
            // Receive echo
            var receiveBuffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(receiveBuffer),
                CancellationToken.None);
            
            var receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
            Console.WriteLine($"📥 Recebido: {receivedMessage}");
            
            await Task.Delay(100);
        }
        
        // Close connection
        await webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Demo completed",
            CancellationToken.None);
        
        Console.WriteLine($"🔌 WebSocket fechado - Estado: {webSocket.State}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro WebSocket: {ex.GetType().Name} - {ex.Message}");
    }
}

static async Task DemonstrarHttp2Features()
{
    using var httpClient = new HttpClient();
    
    // Configure HTTP/2
    httpClient.DefaultRequestVersion = HttpVersion.Version20;
    httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
    
    Console.WriteLine("🚀 Configurado para HTTP/2");
    
    try
    {
        // Test HTTP/2 endpoint
        var response = await httpClient.GetAsync("https://httpbin.org/get");
        
        Console.WriteLine($"📡 Status: {response.StatusCode}");
        Console.WriteLine($"🔢 HTTP Version: {response.Version}");
        Console.WriteLine($"📊 Headers recebidos: {response.Headers.Count()}");
        
        // Read response
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        if (jsonDoc.RootElement.TryGetProperty("headers", out var headers))
        {
            Console.WriteLine("📋 Headers enviados:");
            foreach (var header in headers.EnumerateObject().Take(3))
            {
                Console.WriteLine($"   {header.Name}: {header.Value}");
            }
        }
        
        // Multiple concurrent requests (HTTP/2 multiplexing)
        Console.WriteLine("\n🔄 Testando multiplexing HTTP/2:");
        
        var tasks = Enumerable.Range(1, 3).Select(async i =>
        {
            var url = $"https://httpbin.org/delay/1?request={i}";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var resp = await httpClient.GetAsync(url);
            stopwatch.Stop();
            
            Console.WriteLine($"   Request {i}: {resp.StatusCode} em {stopwatch.ElapsedMilliseconds}ms");
            return stopwatch.ElapsedMilliseconds;
        });
        
        var results = await Task.WhenAll(tasks);
        Console.WriteLine($"⚡ Tempo total concorrente: ~{results.Max()}ms (vs ~{results.Sum()}ms sequencial)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro HTTP/2: {ex.GetType().Name}");
    }
}

static async Task DemonstrarProtocoloCustomizado()
{
    Console.WriteLine("🛠️ Implementando protocolo customizado:");
    
    // Custom protocol: [SIZE][TYPE][DATA]
    var protocol = new CustomProtocol();
    
    // Simulate messages
    var messages = new[]
    {
        new ProtocolMessage { Type = MessageType.Handshake, Data = "ClientHello" },
        new ProtocolMessage { Type = MessageType.Data, Data = "Important data payload" },
        new ProtocolMessage { Type = MessageType.Heartbeat, Data = "ping" },
        new ProtocolMessage { Type = MessageType.Disconnect, Data = "Goodbye" }
    };
    
    foreach (var message in messages)
    {
        var encoded = protocol.EncodeMessage(message);
        Console.WriteLine($"📤 Encoded {message.Type}: {Convert.ToHexString(encoded)} ({encoded.Length} bytes)");
        
        var decoded = protocol.DecodeMessage(encoded);
        Console.WriteLine($"📥 Decoded {decoded.Type}: \"{decoded.Data}\"");
        
        await Task.Delay(50);
    }
}

static async Task DemonstrarDiagnosticosRede()
{
    Console.WriteLine("📊 Diagnósticos de rede:");
    
    // Network interfaces
    var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
        .Take(3);
    
    Console.WriteLine("🌐 Interfaces de rede ativas:");
    foreach (var ni in interfaces)
    {
        Console.WriteLine($"   📡 {ni.Name} ({ni.NetworkInterfaceType})");
        Console.WriteLine($"      🔢 Speed: {ni.Speed / 1_000_000} Mbps");
        Console.WriteLine($"      📊 Bytes sent: {ni.GetIPv4Statistics().BytesSent}");
    }
    
    // DNS resolution
    try
    {
        var hostEntry = await Dns.GetHostEntryAsync("google.com");
        Console.WriteLine($"\n🔍 DNS Resolution para google.com:");
        Console.WriteLine($"   📛 HostName: {hostEntry.HostName}");
        Console.WriteLine($"   🔢 IP Addresses: {hostEntry.AddressList.Length}");
        
        foreach (var ip in hostEntry.AddressList.Take(2))
        {
            Console.WriteLine($"      📍 {ip} ({ip.AddressFamily})");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro DNS: {ex.GetType().Name}");
    }
    
    // Port scanning simulation
    Console.WriteLine("\n🔍 Port scan simulation (localhost):");
    var commonPorts = new[] { 22, 80, 443, 8080 };
    
    var scanTasks = commonPorts.Select(async port =>
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(IPAddress.Loopback, port);
            var timeoutTask = Task.Delay(1000);
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            
            if (completedTask == connectTask && tcpClient.Connected)
            {
                Console.WriteLine($"   ✅ Port {port}: OPEN");
                return (port, true);
            }
            else
            {
                Console.WriteLine($"   ❌ Port {port}: CLOSED/TIMEOUT");
                return (port, false);
            }
        }
        catch
        {
            Console.WriteLine($"   ❌ Port {port}: CLOSED");
            return (port, false);
        }
    });
    
    await Task.WhenAll(scanTasks);
}

static async Task DemonstrarSignalRAvancado()
{
    // Note: This would require a SignalR server running
    // For demo purposes, we'll simulate the client setup
    
    Console.WriteLine("📱 Configuração avançada SignalR Client:");
    
    var connection = new HubConnectionBuilder()
        .WithUrl("https://localhost:5001/chathub") // Simulated URL
        .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
        .Build();
    
    Console.WriteLine($"🔗 Connection criada: {connection.ConnectionId ?? "Not connected"}");
    Console.WriteLine($"📡 Estado: {connection.State}");
    
    // Setup event handlers
    connection.On<string, string>("ReceiveMessage", (user, message) =>
    {
        Console.WriteLine($"📥 Mensagem de {user}: {message}");
    });
    
    connection.Reconnecting += error =>
    {
        Console.WriteLine($"🔄 Reconectando... Erro: {error?.GetType().Name}");
        return Task.CompletedTask;
    };
    
    connection.Reconnected += connectionId =>
    {
        Console.WriteLine($"✅ Reconectado! ID: {connectionId}");
        return Task.CompletedTask;
    };
    
    connection.Closed += error =>
    {
        Console.WriteLine($"🔌 Conexão fechada. Erro: {error?.GetType().Name}");
        return Task.CompletedTask;
    };
    
    Console.WriteLine("⚡ Event handlers configurados");
    Console.WriteLine("📋 Recursos disponíveis:");
    Console.WriteLine("   🔄 Reconexão automática");
    Console.WriteLine("   📨 Mensagens tipadas");
    Console.WriteLine("   🎯 Event handlers");
    Console.WriteLine("   🔐 Streaming (se suportado)");
    
    // Simulate some operations without actually connecting
    await Task.Delay(100);
    
    // Clean up
    await connection.DisposeAsync();
    Console.WriteLine("🧹 SignalR Connection disposed");
}

static async Task DemonstrarPerformanceRede()
{
    Console.WriteLine("⚡ Otimizações de performance de rede:");
    
    // Connection pooling simulation
    var handler = new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        MaxConnectionsPerServer = 10
    };
    
    using var httpClient = new HttpClient(handler);
    
    Console.WriteLine("🔗 Connection pooling configurado:");
    Console.WriteLine($"   ⏰ Lifetime: {handler.PooledConnectionLifetime}");
    Console.WriteLine($"   🔄 Idle timeout: {handler.PooledConnectionIdleTimeout}");
    Console.WriteLine($"   📊 Max connections/server: {handler.MaxConnectionsPerServer}");
    
    // Batch operations simulation
    Console.WriteLine("\n📦 Simulando operações em batch:");
    
    var batchSize = 5;
    var operations = Enumerable.Range(1, batchSize).Select(i => new
    {
        Id = i,
        Data = $"Operation-{i}",
        Timestamp = DateTimeOffset.UtcNow
    }).ToList();
    
    var batchStopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    // Simulate parallel network operations
    var tasks = operations.Select(async op =>
    {
        await Task.Delay(Random.Shared.Next(50, 150)); // Simulate network latency
        return $"Result-{op.Id}";
    });
    
    var results = await Task.WhenAll(tasks);
    batchStopwatch.Stop();
    
    Console.WriteLine($"   ✅ {results.Length} operações completadas em {batchStopwatch.ElapsedMilliseconds}ms");
    Console.WriteLine($"   📊 Throughput: {results.Length * 1000.0 / batchStopwatch.ElapsedMilliseconds:F1} ops/sec");
    
    // Memory efficiency demonstration
    Console.WriteLine("\n💾 Eficiência de memória:");
    
    var beforeMemory = GC.GetTotalMemory(false);
    
    // Use pooled arrays for network buffers
    var bufferPool = System.Buffers.ArrayPool<byte>.Shared;
    var buffer = bufferPool.Rent(8192);
    
    try
    {
        // Simulate buffer usage
        var data = Encoding.UTF8.GetBytes("Network data simulation");
        Array.Copy(data, 0, buffer, 0, data.Length);
        Console.WriteLine($"   📊 Buffer alugado: {buffer.Length} bytes");
        Console.WriteLine($"   📋 Dados copiados: {data.Length} bytes");
        
        await Task.Delay(10);
    }
    finally
    {
        bufferPool.Return(buffer);
        Console.WriteLine("   🔄 Buffer retornado ao pool");
    }
    
    var afterMemory = GC.GetTotalMemory(true);
    Console.WriteLine($"   💾 Memória antes: {beforeMemory:N0} bytes");
    Console.WriteLine($"   💾 Memória depois: {afterMemory:N0} bytes");
    Console.WriteLine($"   📉 Diferença: {afterMemory - beforeMemory:N0} bytes");
}

// Custom Protocol Implementation
public enum MessageType : byte
{
    Handshake = 0x01,
    Data = 0x02,
    Heartbeat = 0x03,
    Disconnect = 0x04
}

public class ProtocolMessage
{
    public MessageType Type { get; set; }
    public string Data { get; set; } = "";
}

public class CustomProtocol
{
    public byte[] EncodeMessage(ProtocolMessage message)
    {
        var dataBytes = Encoding.UTF8.GetBytes(message.Data);
        var result = new byte[5 + dataBytes.Length]; // 4 bytes size + 1 byte type + data
        
        // Write size (4 bytes, big endian)
        var size = dataBytes.Length + 1; // +1 for type byte
        result[0] = (byte)(size >> 24);
        result[1] = (byte)(size >> 16);
        result[2] = (byte)(size >> 8);
        result[3] = (byte)size;
        
        // Write type (1 byte)
        result[4] = (byte)message.Type;
        
        // Write data
        Array.Copy(dataBytes, 0, result, 5, dataBytes.Length);
        
        return result;
    }
    
    public ProtocolMessage DecodeMessage(byte[] data)
    {
        if (data.Length < 5)
            throw new ArgumentException("Invalid message format");
        
        // Read size
        var size = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
        
        // Read type
        var type = (MessageType)data[4];
        
        // Read data
        var dataBytes = new byte[size - 1];
        Array.Copy(data, 5, dataBytes, 0, dataBytes.Length);
        var messageData = Encoding.UTF8.GetString(dataBytes);
        
        return new ProtocolMessage
        {
            Type = type,
            Data = messageData
        };
    }
}

// Network utilities
public static class NetworkUtils
{
    public static async Task<bool> IsPortOpenAsync(string host, int port, int timeoutMs = 3000)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(timeoutMs);
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            return completedTask == connectTask && tcpClient.Connected;
        }
        catch
        {
            return false;
        }
    }
    
    public static async Task<TimeSpan> MeasureLatencyAsync(string host, int port)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        catch
        {
            return TimeSpan.MaxValue;
        }
    }
}

// Bandwidth measurement
public class BandwidthMeter
{
    public async Task<double> MeasureDownloadSpeedAsync(string url, int durationSeconds = 5)
    {
        using var httpClient = new HttpClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var totalBytes = 0L;
        
        try
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await stream.ReadAsync(buffer)) > 0 && 
                   stopwatch.Elapsed.TotalSeconds < durationSeconds)
            {
                totalBytes += bytesRead;
            }
        }
        catch
        {
            return 0;
        }
        
        stopwatch.Stop();
        var seconds = stopwatch.Elapsed.TotalSeconds;
        return totalBytes / seconds / 1024 / 1024; // MB/s
    }
}
