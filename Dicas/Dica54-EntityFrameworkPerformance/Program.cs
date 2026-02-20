using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Dica54_EntityFrameworkPerformance.Data;
using Dica54_EntityFrameworkPerformance.Models;
using Dica54_EntityFrameworkPerformance.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURAÇÃO DO ENTITY FRAMEWORK
// ==========================================

// Configurar Entity Framework com InMemory para demonstrações
builder.Services.AddDbContext<BlogContext>(options =>
{
    options.UseInMemoryDatabase("PerformanceDemo")
           .EnableSensitiveDataLogging() // Para debug
           .EnableDetailedErrors()       // Para debug
           .LogTo(Console.WriteLine, LogLevel.Information); // Log das queries
});

// Registrar serviços
builder.Services.AddScoped<PerformanceDemoService>();
builder.Services.AddScoped<DataSeedService>();

// Configuração de logging
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Swagger para documentação
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// CONFIGURAÇÃO DO PIPELINE
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==========================================
// INICIALIZAÇÃO DOS DADOS
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DataSeedService>();
    await seedService.SeedDataAsync();
    
    var statistics = await seedService.GetDatabaseStatistics();
    Console.WriteLine(statistics);
}

// ==========================================
// ENDPOINTS DE DEMONSTRAÇÃO
// ==========================================

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapGet("/", () => """
🎯 Dica 54: Entity Framework Performance - Demonstrações Disponíveis

📊 COMPARAÇÕES DE PERFORMANCE:
• GET /demo/n1-problem          - Demonstra problema N+1
• GET /demo/eager-loading       - Solução com Eager Loading
• GET /demo/projection          - Solução com Projeção (mais eficiente)

📈 CONSULTAS COMPLEXAS:
• GET /demo/statistics-slow     - Estatísticas sem otimização
• GET /demo/statistics-fast     - Estatísticas otimizadas

📄 PAGINAÇÃO:
• GET /demo/pagination-skip     - Paginação com Skip/Take (página 10)
• GET /demo/pagination-cursor   - Paginação com Cursor (mais eficiente)

🎯 TRACKING:
• GET /demo/with-tracking       - Consulta com tracking
• GET /demo/no-tracking         - Consulta sem tracking (read-only)

💥 CARTESIAN EXPLOSION:
• GET /demo/cartesian-explosion - Problema com múltiplos includes
• GET /demo/split-queries       - Solução com Split Queries

⚡ BULK OPERATIONS:
• POST /demo/update-individual  - Updates individuais (lento)
• POST /demo/update-bulk        - Bulk update (eficiente)

📊 BENCHMARK COMPLETO:
• GET /demo/benchmark           - Executa todos os testes comparativos

📈 INFORMAÇÕES:
• GET /demo/statistics          - Estatísticas do banco de dados

🎮 Acesse /swagger para documentação completa da API
""")
.WithTags("Home")
.WithSummary("Página inicial com todas as demonstrações disponíveis");

// ==========================================
// DEMONSTRAÇÕES DE PROBLEMA N+1
// ==========================================

app.MapGet("/demo/n1-problem", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🐌 Executando demonstração: Problema N+1");
    var result = await service.GetPostsWithN1Problem();
    return Results.Ok(new { 
        message = "Consulta executada com problema N+1 - verifique os logs para ver as múltiplas queries geradas",
        postsReturned = result.Count,
        warning = "⚠️ Esta implementação gera uma query adicional para cada post!"
    });
})
.WithTags("N+1 Problem")
.WithSummary("Demonstra o problema N+1 - múltiplas queries desnecessárias");

app.MapGet("/demo/eager-loading", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🚀 Executando demonstração: Eager Loading");
    var result = await service.GetPostsWithEagerLoading();
    return Results.Ok(new { 
        message = "Consulta executada com Eager Loading - uma única query com JOINs",
        postsReturned = result.Count,
        optimization = "✅ Solução: Include() para carregar dados relacionados em uma query"
    });
})
.WithTags("N+1 Problem")
.WithSummary("Solução com Eager Loading - evita problema N+1");

app.MapGet("/demo/projection", async (PerformanceDemoService service) =>
{
    logger.LogInformation("⚡ Executando demonstração: Projeção");
    var result = await service.GetPostsWithProjection();
    return Results.Ok(new { 
        message = "Consulta executada com Projeção direta para DTO",
        postsReturned = result.Count,
        optimization = "⚡ Melhor solução: Select() direto para DTO carrega apenas campos necessários"
    });
})
.WithTags("N+1 Problem")
.WithSummary("Solução com Projeção - mais eficiente que Eager Loading");

// ==========================================
// DEMONSTRAÇÕES DE CONSULTAS COMPLEXAS
// ==========================================

app.MapGet("/demo/statistics-slow", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🐌 Executando demonstração: Estatísticas sem otimização");
    var result = await service.GetBlogStatisticsNonOptimized();
    return Results.Ok(new { 
        message = "Estatísticas calculadas com múltiplas queries - método não otimizado",
        blogsProcessed = result.Count,
        warning = "⚠️ Cada blog gera múltiplas queries adicionais!"
    });
})
.WithTags("Complex Queries")
.WithSummary("Estatísticas com múltiplas queries - método lento");

app.MapGet("/demo/statistics-fast", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🚀 Executando demonstração: Estatísticas otimizadas");
    var result = await service.GetBlogStatisticsOptimized();
    return Results.Ok(new { 
        message = "Estatísticas calculadas com query otimizada - agregações no banco",
        blogsProcessed = result.Count,
        optimization = "✅ Uma única query com JOINs e agregações"
    });
})
.WithTags("Complex Queries")
.WithSummary("Estatísticas otimizadas - agregações no banco de dados");

// ==========================================
// DEMONSTRAÇÕES DE PAGINAÇÃO
// ==========================================

app.MapGet("/demo/pagination-skip", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🐌 Executando demonstração: Paginação com Skip");
    var result = await service.GetPostsPaginatedNonOptimized(10, 20); // Página 10
    return Results.Ok(new { 
        message = "Paginação executada com Skip/Take - página 10",
        postsReturned = result.Count,
        warning = "⚠️ Skip em páginas altas é custoso para o banco de dados"
    });
})
.WithTags("Pagination")
.WithSummary("Paginação com Skip/Take - problemático para páginas altas");

app.MapGet("/demo/pagination-cursor", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🚀 Executando demonstração: Paginação com Cursor");
    // Simular um cursor (data do último post)
    var cursor = DateTime.UtcNow.AddDays(-30);
    var result = await service.GetPostsPaginatedWithCursor(cursor, 20);
    return Results.Ok(new { 
        message = "Paginação executada com Cursor - mais eficiente",
        postsReturned = result.Count,
        cursor = cursor,
        optimization = "✅ Cursor-based pagination é mais eficiente que Skip/Take"
    });
})
.WithTags("Pagination")
.WithSummary("Paginação com Cursor - mais eficiente para grandes datasets");

// ==========================================
// DEMONSTRAÇÕES DE TRACKING
// ==========================================

app.MapGet("/demo/with-tracking", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🐌 Executando demonstração: Com Tracking");
    var result = await service.GetPostsWithTracking();
    return Results.Ok(new { 
        message = "Consulta executada com tracking ativo",
        postsReturned = result.Count,
        warning = "⚠️ Tracking desnecessário para consultas read-only adiciona overhead"
    });
})
.WithTags("Change Tracking")
.WithSummary("Consulta com tracking - overhead desnecessário para read-only");

app.MapGet("/demo/no-tracking", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🚀 Executando demonstração: Sem Tracking");
    var result = await service.GetPostsWithoutTracking();
    return Results.Ok(new { 
        message = "Consulta executada sem tracking",
        postsReturned = result.Count,
        optimization = "✅ AsNoTracking() remove overhead para consultas read-only"
    });
})
.WithTags("Change Tracking")
.WithSummary("Consulta sem tracking - otimizada para read-only");

// ==========================================
// DEMONSTRAÇÕES DE SPLIT QUERIES
// ==========================================

app.MapGet("/demo/cartesian-explosion", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🐌 Executando demonstração: Cartesian Explosion");
    var result = await service.GetPostsWithCartesianExplosion();
    return Results.Ok(new { 
        message = "Consulta executada com múltiplos includes - Cartesian explosion",
        postsReturned = result.Count,
        warning = "⚠️ Múltiplos JOINs causam duplicação de dados e aumento do tráfego"
    });
})
.WithTags("Split Queries")
.WithSummary("Múltiplos includes causam Cartesian explosion");

app.MapGet("/demo/split-queries", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🚀 Executando demonstração: Split Queries");
    var result = await service.GetPostsWithSplitQueries();
    return Results.Ok(new { 
        message = "Consulta executada com split queries",
        postsReturned = result.Count,
        optimization = "✅ AsSplitQuery() evita Cartesian explosion dividindo em múltiplas queries otimizadas"
    });
})
.WithTags("Split Queries")
.WithSummary("Split queries evitam Cartesian explosion");

// ==========================================
// DEMONSTRAÇÕES DE BULK OPERATIONS
// ==========================================

app.MapPost("/demo/update-individual", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🐌 Executando demonstração: Updates Individuais");
    var postIds = Enumerable.Range(1, 20).ToList(); // Atualizar 20 posts
    await service.UpdatePostViewsIndividually(postIds);
    return Results.Ok(new { 
        message = "Updates executados individualmente",
        postsUpdated = postIds.Count,
        warning = "⚠️ Cada update gera uma transação separada - muito lento"
    });
})
.WithTags("Bulk Operations")
.WithSummary("Updates individuais - uma transação por registro");

app.MapPost("/demo/update-bulk", async (PerformanceDemoService service) =>
{
    logger.LogInformation("🚀 Executando demonstração: Bulk Update");
    var postIds = Enumerable.Range(1, 20).ToList(); // Atualizar 20 posts
    await service.UpdatePostViewsBulk(postIds);
    return Results.Ok(new { 
        message = "Bulk update executado",
        postsUpdated = postIds.Count,
        optimization = "✅ Uma única transação para múltiplos updates"
    });
})
.WithTags("Bulk Operations")
.WithSummary("Bulk update - uma transação para múltiplos registros");

// ==========================================
// BENCHMARK COMPLETO
// ==========================================

app.MapGet("/demo/benchmark", async (PerformanceDemoService service) =>
{
    logger.LogInformation("⚡ Executando benchmark completo...");
    var sw = Stopwatch.StartNew();
    
    var benchmark = await service.RunPerformanceComparison();
    
    sw.Stop();
    
    return Results.Ok(new { 
        message = "Benchmark completo executado",
        totalDuration = sw.Elapsed,
        testResults = benchmark.Results.Select(r => new
        {
            method = r.Method,
            duration = r.Duration.TotalMilliseconds,
            memoryUsed = r.MemoryUsed,
            recordsProcessed = r.RecordsProcessed
        }),
        summary = "Verifique os logs para detalhes das operações executadas"
    });
})
.WithTags("Benchmark")
.WithSummary("Executa benchmark completo comparando todas as técnicas");

// ==========================================
// INFORMAÇÕES DO BANCO
// ==========================================

app.MapGet("/demo/statistics", async (DataSeedService seedService) =>
{
    var statistics = await seedService.GetDatabaseStatistics();
    return Results.Ok(new { 
        message = "Estatísticas do banco de dados",
        statistics = statistics
    });
})
.WithTags("Database Info")
.WithSummary("Mostra estatísticas dos dados no banco");

// ==========================================
// INICIALIZAÇÃO
// ==========================================

logger.LogInformation("🎯 Dica 54: Entity Framework Performance");
logger.LogInformation("🌐 Acesse: http://localhost:5000 para ver todas as demonstrações");
logger.LogInformation("📚 Swagger: http://localhost:5000/swagger");
logger.LogInformation("⚡ Banco de dados inicializado com dados de exemplo");

Console.WriteLine("""

🎯 DICA 54: ENTITY FRAMEWORK PERFORMANCE
═══════════════════════════════════════════════════════════════

🔥 DEMONSTRAÇÕES DISPONÍVEIS:
• N+1 Problem vs Soluções Otimizadas
• Consultas Complexas com Agregações
• Paginação: Skip/Take vs Cursor
• Change Tracking: Com vs Sem Tracking
• Split Queries vs Cartesian Explosion
• Bulk Operations vs Updates Individuais

📊 METRICS DEMONSTRADAS:
• Tempo de Execução
• Consumo de Memória
• Número de Queries Geradas
• Volume de Dados Transferidos

🎮 COMO USAR:
1. Acesse http://localhost:5000 para ver todas as opções
2. Execute cada endpoint para ver os diferentes padrões
3. Compare os logs para entender as diferenças
4. Use /demo/benchmark para teste completo

💡 OBJETIVO: Demonstrar padrões de performance críticos no EF Core
═══════════════════════════════════════════════════════════════

""");

app.Run();
