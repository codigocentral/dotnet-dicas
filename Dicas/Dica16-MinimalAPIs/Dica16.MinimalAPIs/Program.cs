using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IProdutoService, ProdutoService>();

var app = builder.Build();

// Configuração do pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// =================== MINIMAL APIS BÁSICAS ===================

// 1. Endpoint simples com diferentes métodos HTTP
app.MapGet("/", () => "🎉 Bem-vindo às Minimal APIs do .NET!");

app.MapGet("/info", () => new
{
    Aplicacao = "Dica 16: Minimal APIs",
    Versao = "1.0.0",
    Ambiente = app.Environment.EnvironmentName,
    DataHora = DateTime.Now
});

// 2. Endpoints com parâmetros de rota
app.MapGet("/usuarios/{id:int}", (int id) => new
{
    Id = id,
    Nome = $"Usuário {id}",
    Ativo = true
});

app.MapGet("/produtos/{categoria}/{id:int}", (string categoria, int id) => new
{
    Id = id,
    Categoria = categoria,
    Nome = $"Produto {id} da categoria {categoria}",
    Preco = Random.Shared.Next(10, 1000)
});

// 3. Endpoints com query parameters
app.MapGet("/pesquisar", ([FromQuery] string? termo, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10) =>
{
    var resultados = Enumerable.Range(1, tamanho)
        .Select(i => new
        {
            Id = (pagina - 1) * tamanho + i,
            Titulo = $"Resultado {i} para '{termo ?? "todos"}'",
            Relevancia = Random.Shared.NextDouble()
        })
        .ToList();

    return new
    {
        Termo = termo ?? "todos",
        Pagina = pagina,
        TamanhoPagina = tamanho,
        Total = 100,
        Resultados = resultados
    };
});

// =================== CRUD COMPLETO COM MINIMAL APIS ===================

var produtos = new List<Produto>
{
    new(1, "Notebook Dell", "Eletrônicos", 2500.00m),
    new(2, "Mouse Logitech", "Eletrônicos", 120.00m),
    new(3, "Cadeira Ergonômica", "Móveis", 800.00m)
};

// GET - Listar todos os produtos
app.MapGet("/api/produtos", () => Results.Ok(produtos))
   .WithName("ListarProdutos");

// GET - Obter produto por ID
app.MapGet("/api/produtos/{id:int}", (int id) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    return produto is not null ? Results.Ok(produto) : Results.NotFound($"Produto {id} não encontrado");
})
.WithName("ObterProduto");

// POST - Criar novo produto
app.MapPost("/api/produtos", ([FromBody] NovoProdutoRequest request) =>
{
    var novoProduto = new Produto(
        produtos.Max(p => p.Id) + 1,
        request.Nome,
        request.Categoria,
        request.Preco
    );
    produtos.Add(novoProduto);
    return Results.Created($"/api/produtos/{novoProduto.Id}", novoProduto);
})
.WithName("CriarProduto");

// PUT - Atualizar produto
app.MapPut("/api/produtos/{id:int}", (int id, [FromBody] AtualizarProdutoRequest request) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    if (produto is null)
        return Results.NotFound($"Produto {id} não encontrado");

    var produtoAtualizado = produto with
    {
        Nome = request.Nome ?? produto.Nome,
        Categoria = request.Categoria ?? produto.Categoria,
        Preco = request.Preco ?? produto.Preco
    };

    var index = produtos.IndexOf(produto);
    produtos[index] = produtoAtualizado;

    return Results.Ok(produtoAtualizado);
})
.WithName("AtualizarProduto");

// DELETE - Remover produto
app.MapDelete("/api/produtos/{id:int}", (int id) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    if (produto is null)
        return Results.NotFound($"Produto {id} não encontrado");

    produtos.Remove(produto);
    return Results.NoContent();
})
.WithName("RemoverProduto");

// =================== ENDPOINTS AVANÇADOS ===================

// 4. Endpoint com injeção de dependência
app.MapGet("/api/estatisticas", (IProdutoService produtoService) => new
{
    TotalProdutos = produtoService.ContarProdutos(),
    PrecoMedio = produtoService.CalcularPrecoMedio(),
    CategoriasDistintas = produtoService.ObterCategorias(),
    ProdutoMaisCaro = produtoService.ObterMaisCaro(),
    ProdutoMaisBarato = produtoService.ObterMaisBarato()
});

// 5. Endpoint com validação customizada
app.MapPost("/api/validar-produto", ([FromBody] NovoProdutoRequest request) =>
{
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(request);
    
    if (!Validator.TryValidateObject(request, context, validationResults, true))
    {
        return Results.BadRequest(new
        {
            Erros = validationResults.Select(vr => vr.ErrorMessage).ToList()
        });
    }

    return Results.Ok(new { Mensagem = "Produto válido!", Produto = request });
})
.WithName("ValidarProduto");

// 6. Endpoint com filtros complexos
app.MapGet("/api/produtos/filtrar", 
    ([FromQuery] string? categoria, 
     [FromQuery] decimal? precoMin, 
     [FromQuery] decimal? precoMax,
     [FromQuery] string? ordenarPor = "nome") =>
{
    var query = produtos.AsEnumerable();

    if (!string.IsNullOrEmpty(categoria))
        query = query.Where(p => p.Categoria.Contains(categoria, StringComparison.OrdinalIgnoreCase));

    if (precoMin.HasValue)
        query = query.Where(p => p.Preco >= precoMin.Value);

    if (precoMax.HasValue)
        query = query.Where(p => p.Preco <= precoMax.Value);

    query = ordenarPor?.ToLower() switch
    {
        "preco" => query.OrderBy(p => p.Preco),
        "categoria" => query.OrderBy(p => p.Categoria),
        _ => query.OrderBy(p => p.Nome)
    };

    return Results.Ok(query.ToList());
})
.WithName("FiltrarProdutos");

// =================== ENDPOINTS COM DIFERENTES TIPOS DE RESPOSTA ===================

// 7. Endpoint que retorna arquivo
app.MapGet("/api/produtos/export", () =>
{
    var csv = "Id,Nome,Categoria,Preco\n" +
              string.Join("\n", produtos.Select(p => $"{p.Id},{p.Nome},{p.Categoria},{p.Preco}"));
    
    return Results.File(
        System.Text.Encoding.UTF8.GetBytes(csv),
        "text/csv",
        "produtos.csv"
    );
})
.WithName("ExportarProdutos");

// 8. Endpoint com diferentes códigos de status
app.MapPost("/api/produtos/{id:int}/ativar", (int id) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    
    return produto switch
    {
        null => Results.NotFound(new { Erro = "Produto não encontrado" }),
        _ => Results.Ok(new { Mensagem = $"Produto {produto.Nome} ativado com sucesso!" })
    };
})
.WithName("AtivarProduto");

// =================== AGRUPAMENTO DE ROTAS ===================

// 9. Agrupamento com MapGroup
var categoriasGroup = app.MapGroup("/api/categorias")
                        .WithTags("Categorias");

categoriasGroup.MapGet("/", () => produtos.Select(p => p.Categoria).Distinct().ToList());

categoriasGroup.MapGet("/{categoria}/produtos", (string categoria) =>
    produtos.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList());

categoriasGroup.MapGet("/{categoria}/total", (string categoria) =>
    produtos.Count(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)));

// =================== MIDDLEWARE CUSTOMIZADO ===================

// 10. Middleware simples para logging
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    await next(context);
    var duration = DateTime.UtcNow - start;
    
    Console.WriteLine($"[{start:HH:mm:ss}] {context.Request.Method} {context.Request.Path} " +
                     $"→ {context.Response.StatusCode} ({duration.TotalMilliseconds:F2}ms)");
});

Console.WriteLine("🚀 Minimal API iniciada!");
Console.WriteLine("📚 Documentação Swagger: https://localhost:7299/swagger");
Console.WriteLine("🔗 Endpoints disponíveis:");
Console.WriteLine("   GET  /                      - Página inicial");
Console.WriteLine("   GET  /info                  - Informações da aplicação");
Console.WriteLine("   GET  /api/produtos          - Listar produtos");
Console.WriteLine("   POST /api/produtos          - Criar produto");
Console.WriteLine("   GET  /api/produtos/{id}     - Obter produto por ID");
Console.WriteLine("   PUT  /api/produtos/{id}     - Atualizar produto");
Console.WriteLine("   DELETE /api/produtos/{id}   - Remover produto");
Console.WriteLine("   GET  /api/estatisticas      - Estatísticas dos produtos");

app.Run();

// =================== MODELS E DTOs ===================

public record Produto(int Id, string Nome, string Categoria, decimal Preco);

public record NovoProdutoRequest(
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
    string Nome,
    
    [Required(ErrorMessage = "Categoria é obrigatória")]
    string Categoria,
    
    [Range(0.01, 999999.99, ErrorMessage = "Preço deve ser maior que zero")]
    decimal Preco
);

public record AtualizarProdutoRequest(string? Nome, string? Categoria, decimal? Preco);

// =================== SERVICES ===================

public interface IProdutoService
{
    int ContarProdutos();
    decimal CalcularPrecoMedio();
    List<string> ObterCategorias();
    Produto? ObterMaisCaro();
    Produto? ObterMaisBarato();
}

public class ProdutoService : IProdutoService
{
    private readonly List<Produto> _produtos = new()
    {
        new(1, "Notebook Dell", "Eletrônicos", 2500.00m),
        new(2, "Mouse Logitech", "Eletrônicos", 120.00m),
        new(3, "Cadeira Ergonômica", "Móveis", 800.00m),
        new(4, "Monitor 4K", "Eletrônicos", 1200.00m),
        new(5, "Mesa de Escritório", "Móveis", 450.00m)
    };

    public int ContarProdutos() => _produtos.Count;
    
    public decimal CalcularPrecoMedio() => _produtos.Average(p => p.Preco);
    
    public List<string> ObterCategorias() => _produtos.Select(p => p.Categoria).Distinct().ToList();
    
    public Produto? ObterMaisCaro() => _produtos.OrderByDescending(p => p.Preco).FirstOrDefault();
    
    public Produto? ObterMaisBarato() => _produtos.OrderBy(p => p.Preco).FirstOrDefault();
}
