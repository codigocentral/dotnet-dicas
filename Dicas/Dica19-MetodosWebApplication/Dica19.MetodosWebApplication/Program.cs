/*
 * Dica 19: Métodos WebApplication (Run, Use, Map)
 * 
 * No ASP.NET Core, o pipeline de middleware é configurado usando três métodos principais:
 * 
 * - Run(): Terminal middleware - encerra o pipeline (não chama next)
 * - Use(): Middleware intermediário - pode chamar next() para continuar
 * - Map(): Cria branchs no pipeline baseado em caminhos
 * 
 * A ORDEM é fundamental! Middlewares são executados na ordem em que são adicionados.
 */

using Microsoft.OpenApi.Models;

Console.WriteLine("==== Dica 19: Métodos WebApplication (Run, Use, Map) ====");
Console.WriteLine("Esta dica demonstra os métodos fundamentais da WebApplication:");
Console.WriteLine("Run, Use e Map - e a importância da ordem de execução.\n");

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Dica 19 - Métodos WebApplication API", 
        Version = "v1",
        Description = "Demonstração dos métodos Run, Use e Map"
    });
});

var app = builder.Build();

Console.WriteLine("1. Configurando pipeline de middleware...");

// ========================================
// 1. USE - Middleware geral no pipeline
// ========================================
Console.WriteLine("   • Adicionando middleware Use (logging de requisições)");

app.Use(async (context, next) =>
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] ➡️  USE: Requisição recebida - {context.Request.Method} {context.Request.Path}");
    
    // Chamada do próximo middleware na pipeline
    await next();
    
    Console.WriteLine($"[{timestamp}] ⬅️  USE: Resposta enviada - Status: {context.Response.StatusCode}");
});

// ========================================
// 2. USE - Middleware de autenticação simulada
// ========================================
Console.WriteLine("   • Adicionando middleware Use (autenticação simulada)");

app.Use(async (context, next) =>
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] 🔐 USE: Verificando autenticação...");
    
    // Simular verificação de token
    var hasToken = context.Request.Headers.ContainsKey("Authorization");
    if (!hasToken && context.Request.Path.StartsWithSegments("/api/secure"))
    {
        Console.WriteLine($"[{timestamp}] ❌ USE: Acesso negado - token não encontrado");
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Token de autorização necessário");
        return; // Não chama next() - pipeline é interrompida
    }
    
    Console.WriteLine($"[{timestamp}] ✅ USE: Autenticação ok, prosseguindo...");
    await next();
});

// ========================================
// 3. MAP - Mapeamento de rotas específicas
// ========================================
Console.WriteLine("   • Adicionando mapeamentos Map específicos");

// Map para rota específica /api/info
app.Map("/api/info", infoApp =>
{
    Console.WriteLine("     - Configurando /api/info");
    
    infoApp.Use(async (context, next) =>
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.WriteLine($"[{timestamp}] 📍 MAP(/api/info): Middleware específico executado");
        await next();
    });
    
    // Run dentro do Map - terminal para esta rota específica
    infoApp.Run(async context =>
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.WriteLine($"[{timestamp}] 🎯 MAP(/api/info) + RUN: Processando requisição");
        
        var info = new
        {
            Aplicacao = "Dica 19 - Métodos WebApplication",
            Versao = "1.0.0",
            Timestamp = DateTime.Now,
            Metodo = context.Request.Method,
            Path = context.Request.Path.Value,
            UserAgent = context.Request.Headers.UserAgent.ToString()
        };
        
        await context.Response.WriteAsJsonAsync(info);
    });
});

// Map para rota /api/secure (protegida)
app.Map("/api/secure", secureApp =>
{
    Console.WriteLine("     - Configurando /api/secure (protegida)");
    
    secureApp.Run(async context =>
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.WriteLine($"[{timestamp}] 🔒 MAP(/api/secure) + RUN: Área protegida acessada");
        
        var secureData = new
        {
            Mensagem = "Dados sensíveis - acesso autorizado",
            Usuario = "admin",
            Timestamp = DateTime.Now,
            DadosSecretos = new[] { "config1", "config2", "config3" }
        };
        
        await context.Response.WriteAsJsonAsync(secureData);
    });
});

// ========================================
// 4. Endpoints da API Minimal
// ========================================
Console.WriteLine("   • Configurando endpoints da API");

// Swagger/OpenAPI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dica 19 API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

// Endpoint GET raiz
app.MapGet("/", () =>
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] 🏠 ENDPOINT: Página inicial acessada");
    
    return Results.Json(new
    {
        Titulo = "Dica 19: Métodos WebApplication (Run, Use, Map)",
        Descricao = "Demonstração da ordem de execução dos métodos",
        Endpoints = new[]
        {
            "GET / - Esta página",
            "GET /api/info - Informações da aplicação",
            "GET /api/secure - Área protegida (requer header Authorization)",
            "GET /api/usuarios - Lista de usuários",
            "POST /api/usuarios - Criar usuário",
            "GET /test/pipeline - Teste do pipeline de middleware"
        },
        Documentacao = "Acesse /swagger para ver a documentação da API"
    });
});

// Endpoints de usuários
app.MapGet("/api/usuarios", () =>
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] 👥 ENDPOINT: Listando usuários");
    
    return Results.Json(new[]
    {
        new { Id = 1, Nome = "João Silva", Email = "joao@email.com", Ativo = true },
        new { Id = 2, Nome = "Maria Santos", Email = "maria@email.com", Ativo = true },
        new { Id = 3, Nome = "Pedro Costa", Email = "pedro@email.com", Ativo = false }
    });
});

app.MapPost("/api/usuarios", (object usuario) =>
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] ➕ ENDPOINT: Criando usuário - {usuario}");
    
    return Results.Json(new
    {
        Sucesso = true,
        Mensagem = "Usuário criado com sucesso",
        Timestamp = DateTime.Now,
        DadosRecebidos = usuario
    });
});

// Endpoint para testar o pipeline
app.MapGet("/test/pipeline", () =>
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] 🧪 ENDPOINT: Teste do pipeline");
    
    return Results.Json(new
    {
        Mensagem = "Pipeline de middleware executado com sucesso!",
        Observacao = "Verifique o console para ver a ordem de execução dos middlewares",
        Timestamp = DateTime.Now
    });
});

// ========================================
// 5. RUN - Middleware terminal (fallback)
// ========================================
Console.WriteLine("   • Adicionando middleware Run (terminal/fallback)");

// IMPORTANTE: Run deve ser sempre o último!
app.Run(async context =>
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] 🚫 RUN: Rota não encontrada - {context.Request.Path}");
    
    context.Response.StatusCode = 404;
    await context.Response.WriteAsJsonAsync(new
    {
        Erro = "Rota não encontrada",
        Path = context.Request.Path.Value,
        Metodo = context.Request.Method,
        Sugestoes = new[]
        {
            "GET /",
            "GET /api/info", 
            "GET /api/usuarios",
            "GET /test/pipeline"
        },
        Timestamp = DateTime.Now
    });
});

Console.WriteLine("\n=== Ordem de Execução dos Métodos ===");
Console.WriteLine("1. USE (logging) - sempre executa");
Console.WriteLine("2. USE (autenticação) - pode interromper o pipeline");
Console.WriteLine("3. MAP (rotas específicas) - executa apenas se a rota corresponder");
Console.WriteLine("4. Endpoints da API - executa se encontrar correspondência");
Console.WriteLine("5. RUN (fallback) - executa apenas se nenhuma correspondência anterior");

Console.WriteLine("\n=== Demonstração Iniciada ===");
Console.WriteLine("🌐 Servidor rodando em: http://localhost:5000");
Console.WriteLine("📚 Documentação Swagger: http://localhost:5000/swagger");
Console.WriteLine("🧪 Teste o pipeline com diferentes URLs e observe a ordem de execução!");
Console.WriteLine("\nExemplos para testar:");
Console.WriteLine("• curl http://localhost:5000/");
Console.WriteLine("• curl http://localhost:5000/api/info");
Console.WriteLine("• curl http://localhost:5000/api/secure");
Console.WriteLine("• curl -H \"Authorization: Bearer token123\" http://localhost:5000/api/secure");
Console.WriteLine("• curl http://localhost:5000/rota-inexistente");
Console.WriteLine("\nPressione Ctrl+C para parar o servidor.");

// Não chega aqui por causa do app.Run() acima
