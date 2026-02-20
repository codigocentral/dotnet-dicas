# 🌐 Dica 94: Advanced Networking & Protocols (.NET 9)

## 📋 Sobre

Esta dica demonstra técnicas avançadas de networking e protocolos em .NET 9, incluindo TCP/UDP sockets, WebSockets, HTTP/2, protocolos customizados, diagnósticos de rede, SignalR e otimizações de performance.

## 🎯 Conceitos Demonstrados

### 1. 🔌 TCP Socket Programming

- TcpListener e TcpClient
- Comunicação bidirecional
- Stream handling
- Client-server architecture

### 2. 📡 UDP Socket Programming

- UdpClient para comunicação UDP
- Envio e recebimento de datagramas
- Comunicação sem conexão
- EndPoint management

### 3. 🔗 WebSocket Client

- ClientWebSocket para comunicação real-time
- Handshake e headers customizados
- Envio e recebimento de mensagens
- Gerenciamento de estado da conexão

### 4. 🚀 HTTP/2 Advanced Features

- HttpClient configurado para HTTP/2
- Request multiplexing
- Performance comparison
- Version negotiation

### 5. 🛠️ Custom Network Protocol

- Protocolo binário customizado
- Encoding/decoding de mensagens
- Message framing
- Type safety

### 6. 📊 Network Diagnostics

- NetworkInterface information
- DNS resolution
- Port scanning
- Network monitoring

### 7. 📱 SignalR Client Advanced

- HubConnectionBuilder configuration
- Automatic reconnection
- Event handlers
- Connection management

### 8. ⚡ Network Performance

- Connection pooling
- Batch operations
- ArrayPool para buffers
- Memory optimization

## 🚀 Como Executar

```bash
dotnet run
```

## 📊 Saída Esperada

```
🌐 Dica 94: Advanced Networking & Protocols (.NET 9)
====================================================

1. 🔌 TCP Socket Programming:
─────────────────────────────
🖥️ TCP Server iniciado em 127.0.0.1:8080
📤 Cliente enviou: Hello TCP Server!
✅ Cliente conectado: 127.0.0.1:50958
📥 Servidor recebeu: Hello TCP Server!
📤 Servidor respondeu: Echo: Hello TCP Server!
📥 Cliente recebeu: Echo: Hello TCP Server!
🛑 TCP Server parado

2. 📡 UDP Socket Programming:
─────────────────────────────
📡 UDP Server iniciado na porta 8081
📤 UDP Cliente enviou: UDP Message 1
📥 UDP Servidor recebeu: UDP Message 1 de 127.0.0.1:60373
📤 UDP Servidor respondeu: ACK
📤 UDP Cliente enviou: UDP Message 2
📥 UDP Servidor recebeu: UDP Message 2 de 127.0.0.1:60373
📤 UDP Servidor respondeu: ACK
📤 UDP Cliente enviou: UDP Message 3
📥 UDP Servidor recebeu: UDP Message 3 de 127.0.0.1:60373
📤 UDP Servidor respondeu: ACK
🛑 UDP Server finalizado

3. 🔗 WebSocket Client:
──────────────────────
🔗 Conectando WebSocket: wss://echo.websocket.org/
✅ WebSocket conectado - Estado: Open
📤 Enviado: Hello WebSocket!
📥 Recebido: Request served by d56832234ce08e
📤 Enviado: This is a test message
📥 Recebido: Hello WebSocket!
📤 Enviado: WebSocket communication works!
📥 Recebido: This is a test message
🔌 WebSocket fechado - Estado: Closed

4. 🚀 HTTP/2 Advanced Features:
───────────────────────────────
🚀 Configurado para HTTP/2
📡 Status: OK
🔢 HTTP Version: 2.0
📊 Headers recebidos: 4
📋 Headers enviados:
   Host: httpbin.org
   X-Amzn-Trace-Id: Root=1-687132fd-27813d660325ca976bc2b134

🔄 Testando multiplexing HTTP/2:
   Request 2: BadGateway em 723ms
   Request 1: OK em 3219ms
   Request 3: OK em 3867ms
⚡ Tempo total concorrente: ~3867ms (vs ~7809ms sequencial)

5. 🛠️ Custom Network Protocol:
──────────────────────────────
🛠️ Implementando protocolo customizado:
📤 Encoded Handshake: 0000000C01436C69656E7448656C6C6F (16 bytes)
📥 Decoded Handshake: "ClientHello"
📤 Encoded Data: 0000001702496D706F7274616E742064617461207061796C6F6164 (27 bytes)
📥 Decoded Data: "Important data payload"
📤 Encoded Heartbeat: 000000050370696E67 (9 bytes)
📥 Decoded Heartbeat: "ping"
📤 Encoded Disconnect: 0000000804476F6F64627965 (12 bytes)
📥 Decoded Disconnect: "Goodbye"

6. 📊 Network Diagnostics:
──────────────────────────
📊 Diagnósticos de rede:
🌐 Interfaces de rede ativas:
   📡 lo0 (Loopback)
      🔢 Speed: 0 Mbps
      📊 Bytes sent: 20072448
   📡 en0 (Wireless80211)
      🔢 Speed: 71 Mbps
      📊 Bytes sent: 1324250112
   📡 awdl0 (Wireless80211)
      🔢 Speed: 0 Mbps
      📊 Bytes sent: 4133888

🔍 DNS Resolution para google.com:
   📛 HostName: google.com
   🔢 IP Addresses: 2
      📍 2800:3f0:4001:836::200e (InterNetworkV6)
      📍 172.217.30.78 (InterNetwork)

🔍 Port scan simulation (localhost):
   ❌ Port 22: CLOSED/TIMEOUT
   ❌ Port 80: CLOSED/TIMEOUT
   ❌ Port 443: CLOSED/TIMEOUT
   ❌ Port 8080: CLOSED/TIMEOUT

7. 📱 SignalR Client Advanced:
─────────────────────────────
📱 Configuração avançada SignalR Client:
🔗 Connection criada: Not connected
📡 Estado: Disconnected
⚡ Event handlers configurados
📋 Recursos disponíveis:
   🔄 Reconexão automática
   📨 Mensagens tipadas
   🎯 Event handlers
   🔐 Streaming (se suportado)
🧹 SignalR Connection disposed

8. ⚡ Network Performance:
─────────────────────────────
⚡ Otimizações de performance de rede:
🔗 Connection pooling configurado:
   ⏰ Lifetime: 00:02:00
   🔄 Idle timeout: 00:01:00
   📊 Max connections/server: 10

📦 Simulando operações em batch:
   ✅ 5 operações completadas em 148ms
   📊 Throughput: 33.8 ops/sec

💾 Eficiência de memória:
   📊 Buffer alugado: 8192 bytes
   📋 Dados copiados: 23 bytes
   🔄 Buffer retornado ao pool
   💾 Memória antes: 1,662,224 bytes
   💾 Memória depois: 970,808 bytes
   📉 Diferença: -691,416 bytes

✅ Demonstração completa de Advanced Networking!
```

## 🔧 Funcionalidades

### TCP Sockets

- ✅ TcpListener/TcpClient
- ✅ Comunicação bidirecional
- ✅ Stream async operations
- ✅ Connection management

### UDP Sockets

- ✅ UdpClient operations
- ✅ Datagram communication
- ✅ Broadcast/multicast
- ✅ Connectionless protocol

### WebSockets

- ✅ ClientWebSocket
- ✅ Real-time communication
- ✅ Message framing
- ✅ Connection state management

### HTTP/2

- ✅ Version negotiation
- ✅ Request multiplexing
- ✅ Performance optimization
- ✅ Header compression

### Custom Protocol

- ✅ Binary protocol design
- ✅ Message encoding/decoding
- ✅ Type-safe operations
- ✅ Protocol versioning

### Network Diagnostics

- ✅ Interface enumeration
- ✅ DNS resolution
- ✅ Port scanning
- ✅ Network statistics

### SignalR Client

- ✅ Hub connection
- ✅ Automatic reconnection
- ✅ Type-safe messaging
- ✅ Event handling

### Performance

- ✅ Connection pooling
- ✅ Buffer pooling
- ✅ Batch operations
- ✅ Memory optimization

## 🎓 Conceitos Aprendidos

- **TCP/UDP**: Diferenças entre protocolos confiáveis e não-confiáveis
- **WebSockets**: Comunicação full-duplex em tempo real
- **HTTP/2**: Multiplexing e otimizações de performance
- **Custom Protocols**: Design de protocolos binários eficientes
- **Network Diagnostics**: Monitoramento e troubleshooting
- **Connection Pooling**: Reutilização de conexões para performance
- **Buffer Management**: Uso eficiente de memória em operações de rede

## 📚 Referências

- [.NET Networking](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/)
- [TcpClient Class](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient)
- [UdpClient Class](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.udpclient)
- [ClientWebSocket](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket)
- [HttpClient HTTP/2](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
- [SignalR Client](https://docs.microsoft.com/en-us/aspnet/core/signalr/dotnet-client)
