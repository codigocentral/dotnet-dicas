using Dica56_gRPC.Services;
using Dica56_gRPC.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURAÇÃO DOS SERVIÇOS
// ==========================================

// Adicionar serviços gRPC
builder.Services.AddGrpc(options =>
{
    // Configurações globais do gRPC
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaxReceiveMessageSize = 4 * 1024 * 1024; // 4MB
    options.MaxSendMessageSize = 4 * 1024 * 1024; // 4MB
});

// Adicionar suporte para gRPC-Web (permite chamadas do browser)
builder.Services.AddGrpcReflection();

// Registrar repositórios
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

// Configurar logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Configurar CORS para gRPC-Web
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

var app = builder.Build();

// ==========================================
// CONFIGURAÇÃO DO PIPELINE
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    // Habilitar reflexão gRPC em desenvolvimento
    app.MapGrpcReflectionService();
}

// Habilitar CORS
app.UseCors("AllowAll");

// Habilitar roteamento
app.UseRouting();

// ==========================================
// MAPEAMENTO DOS SERVIÇOS gRPC
// ==========================================

// Serviço básico de saudação (demonstra streaming)
app.MapGrpcService<GreeterService>();

// Serviço de produtos (demonstra CRUD)
app.MapGrpcService<ProductGrpcService>();

// Serviço de streaming (demonstra funcionalidades avançadas)
app.MapGrpcService<StreamingGrpcService>();

// ==========================================
// ENDPOINT DE INFORMAÇÕES
// ==========================================

app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("""
    🚀 Dica 56: gRPC com ASP.NET Core
    
    📡 Serviços gRPC Disponíveis:
    
    🔹 Greeter Service (greet.Greeter)
       • SayHello - Chamada unária simples
       • SayHelloServerStreaming - Server streaming
       • SayHelloClientStreaming - Client streaming  
       • SayHelloBidirectional - Bidirectional streaming
    
    🔹 Product Service (products.ProductService)
       • GetProduct - Obter produto por ID
       • GetProducts - Listar todos os produtos
       • CreateProduct - Criar novo produto
       • UpdateProduct - Atualizar produto
       • DeleteProduct - Deletar produto
       • SearchProducts - Buscar produtos
    
    🔹 Streaming Service (streaming.StreamingService)
       • StreamData - Stream de dados em tempo real
       • UploadFile - Upload de arquivo em chunks
       • Chat - Chat bidirectional
       • MonitorMetrics - Monitoramento de métricas
    
    📚 Como usar:
    1. Use um cliente gRPC como grpcurl, BloomRPC ou Postman
    2. Conecte em: http://localhost:5056
    3. Para reflexão: grpcurl -plaintext localhost:5056 list
    
    💡 Funcionalidades demonstradas:
    • Chamadas unárias e streaming
    • Protocol Buffers (protobuf)
    • Validação e tratamento de erros
    • Interceptors e logging
    • Performance e compressão
    • Estruturação de serviços
    
    🔧 Tecnologias:
    • ASP.NET Core 9.0
    • Grpc.AspNetCore 2.66.0
    • Google.Protobuf 3.28.2
    • Protocol Buffers v3
    
    🧪 TESTES DISPONÍVEIS:
    • Execute o script ./demo-grpc.sh para demonstração
    • Use grpcurl para testes via linha de comando
    • Use BloomRPC ou Postman para interface gráfica
    """);
});

// ==========================================
// INICIAR APLICAÇÃO
// ==========================================

Console.WriteLine("🚀 Iniciando servidor gRPC...");
Console.WriteLine("📡 Porta: 5056");
Console.WriteLine("🌐 URL: http://localhost:5056");
Console.WriteLine("📋 Reflexão habilitada em desenvolvimento");
Console.WriteLine("💡 Acesse / para informações dos serviços");
Console.WriteLine();

try
{
    app.Run("http://localhost:5056");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erro ao iniciar servidor: {ex.Message}");
    throw;
}
