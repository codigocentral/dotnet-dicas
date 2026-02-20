using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Dica54_EntityFrameworkPerformance.Data;
using Dica54_EntityFrameworkPerformance.Models;

namespace Dica54_EntityFrameworkPerformance.Services;

/// <summary>
/// Serviço para demonstrar problemas de performance e suas soluções
/// </summary>
public class PerformanceDemoService
{
    private readonly BlogContext _context;
    private readonly ILogger<PerformanceDemoService> _logger;

    public PerformanceDemoService(BlogContext context, ILogger<PerformanceDemoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ==========================================
    // DEMONSTRAÇÕES DE PROBLEMAS N+1
    // ==========================================

    /// <summary>
    /// PROBLEMA: N+1 Query Problem - Carrega dados sem includes
    /// </summary>
    public async Task<List<PostSummaryDto>> GetPostsWithN1Problem()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🐌 INICIANDO: Consulta com problema N+1");

        // ❌ PROBLEMÁTICO: Cada acesso a navigation property gera uma nova query
        var posts = await _context.Posts
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .ToListAsync();

        var result = posts.Select(p => new PostSummaryDto
        {
            Id = p.Id,
            Title = p.Title,
            Summary = p.Summary,
            CreatedAt = p.CreatedAt,
            AuthorName = p.Author.Name, // ❌ Query extra para cada post
            CategoryName = p.Category?.Name, // ❌ Query extra para cada post
            ViewCount = p.ViewCount,
            LikeCount = p.LikeCount,
            CommentCount = p.Comments.Count // ❌ Query extra para cada post
        }).ToList();

        sw.Stop();
        _logger.LogWarning("🐌 CONCLUÍDO: N+1 Problem em {Duration}ms - {PostCount} posts, múltiplas queries geradas", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    /// <summary>
    /// SOLUÇÃO: Eager Loading com Include para evitar N+1
    /// </summary>
    public async Task<List<PostSummaryDto>> GetPostsWithEagerLoading()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🚀 INICIANDO: Consulta com Eager Loading");

        // ✅ OTIMIZADO: Uma única query com JOINs
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Comments)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .ToListAsync();

        var result = posts.Select(p => new PostSummaryDto
        {
            Id = p.Id,
            Title = p.Title,
            Summary = p.Summary,
            CreatedAt = p.CreatedAt,
            AuthorName = p.Author.Name,
            CategoryName = p.Category?.Name,
            ViewCount = p.ViewCount,
            LikeCount = p.LikeCount,
            CommentCount = p.Comments.Count
        }).ToList();

        sw.Stop();
        _logger.LogInformation("🚀 CONCLUÍDO: Eager Loading em {Duration}ms - {PostCount} posts, 1 query", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    /// <summary>
    /// SOLUÇÃO ALTERNATIVA: Projeção direta para DTO (mais eficiente)
    /// </summary>
    public async Task<List<PostSummaryDto>> GetPostsWithProjection()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("⚡ INICIANDO: Consulta com Projeção");

        // ✅ MAIS OTIMIZADO: Projeção direta no banco, carrega apenas campos necessários
        var result = await _context.Posts
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new PostSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                Summary = p.Summary,
                CreatedAt = p.CreatedAt,
                AuthorName = p.Author.Name,
                CategoryName = p.Category != null ? p.Category.Name : null,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                CommentCount = p.Comments.Count()
            })
            .ToListAsync();

        sw.Stop();
        _logger.LogInformation("⚡ CONCLUÍDO: Projeção em {Duration}ms - {PostCount} posts, campos otimizados", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    // ==========================================
    // DEMONSTRAÇÕES DE CONSULTAS COMPLEXAS
    // ==========================================

    /// <summary>
    /// PROBLEMA: Consulta complexa sem otimização
    /// </summary>
    public async Task<List<BlogStatisticsDto>> GetBlogStatisticsNonOptimized()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🐌 INICIANDO: Estatísticas sem otimização");

        // ❌ PROBLEMÁTICO: Múltiplas consultas e processamento em memória
        var blogs = await _context.Blogs.ToListAsync();
        var result = new List<BlogStatisticsDto>();

        foreach (var blog in blogs)
        {
            var posts = await _context.Posts
                .Where(p => p.BlogId == blog.Id)
                .ToListAsync();

            var comments = await _context.Comments
                .Where(c => posts.Select(p => p.Id).Contains(c.PostId))
                .ToListAsync();

            result.Add(new BlogStatisticsDto
            {
                Id = blog.Id,
                Title = blog.Title,
                PostCount = posts.Count,
                TotalViews = posts.Sum(p => p.ViewCount),
                TotalLikes = posts.Sum(p => p.LikeCount),
                TotalComments = comments.Count,
                LastPostDate = posts.Any() ? posts.Max(p => p.CreatedAt) : DateTime.MinValue
            });
        }

        sw.Stop();
        _logger.LogWarning("🐌 CONCLUÍDO: Estatísticas sem otimização em {Duration}ms - {BlogCount} blogs", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    /// <summary>
    /// SOLUÇÃO: Consulta otimizada com GROUP BY e agregações
    /// </summary>
    public async Task<List<BlogStatisticsDto>> GetBlogStatisticsOptimized()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🚀 INICIANDO: Estatísticas otimizadas");

        // ✅ OTIMIZADO: Uma única query com JOINs e agregações
        var result = await _context.Blogs
            .Select(b => new BlogStatisticsDto
            {
                Id = b.Id,
                Title = b.Title,
                PostCount = b.Posts.Count(),
                TotalViews = b.Posts.Sum(p => p.ViewCount),
                TotalLikes = b.Posts.Sum(p => p.LikeCount),
                TotalComments = b.Posts.SelectMany(p => p.Comments).Count(),
                LastPostDate = b.Posts.Any() ? b.Posts.Max(p => p.CreatedAt) : DateTime.MinValue
            })
            .ToListAsync();

        sw.Stop();
        _logger.LogInformation("🚀 CONCLUÍDO: Estatísticas otimizadas em {Duration}ms - {BlogCount} blogs", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    // ==========================================
    // DEMONSTRAÇÕES DE PAGINAÇÃO
    // ==========================================

    /// <summary>
    /// PROBLEMA: Paginação com Skip/Take sem otimização
    /// </summary>
    public async Task<List<PostSummaryDto>> GetPostsPaginatedNonOptimized(int page, int pageSize)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🐌 INICIANDO: Paginação sem otimização - Página {Page}, Tamanho {PageSize}", page, pageSize);

        // ❌ PROBLEMÁTICO: Skip em páginas altas é custoso
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = posts.Select(p => new PostSummaryDto
        {
            Id = p.Id,
            Title = p.Title,
            Summary = p.Summary,
            CreatedAt = p.CreatedAt,
            AuthorName = p.Author.Name,
            CategoryName = p.Category?.Name,
            ViewCount = p.ViewCount,
            LikeCount = p.LikeCount
        }).ToList();

        sw.Stop();
        _logger.LogWarning("🐌 CONCLUÍDO: Paginação sem otimização em {Duration}ms - {PostCount} posts", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    /// <summary>
    /// SOLUÇÃO: Paginação com cursor (mais eficiente para páginas altas)
    /// </summary>
    public async Task<List<PostSummaryDto>> GetPostsPaginatedWithCursor(DateTime? cursor, int pageSize)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🚀 INICIANDO: Paginação com cursor - Cursor {Cursor}, Tamanho {PageSize}", cursor, pageSize);

        // ✅ OTIMIZADO: Usa cursor ao invés de Skip para melhor performance
        var query = _context.Posts
            .Where(p => p.IsPublished);

        if (cursor.HasValue)
        {
            query = query.Where(p => p.CreatedAt < cursor.Value);
        }

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .Take(pageSize)
            .Select(p => new PostSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                Summary = p.Summary,
                CreatedAt = p.CreatedAt,
                AuthorName = p.Author.Name,
                CategoryName = p.Category != null ? p.Category.Name : null,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount
            })
            .ToListAsync();

        sw.Stop();
        _logger.LogInformation("🚀 CONCLUÍDO: Paginação com cursor em {Duration}ms - {PostCount} posts", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    // ==========================================
    // DEMONSTRAÇÕES DE BULK OPERATIONS
    // ==========================================

    /// <summary>
    /// PROBLEMA: Updates individuais (lento)
    /// </summary>
    public async Task UpdatePostViewsIndividually(List<int> postIds)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🐌 INICIANDO: Updates individuais - {PostCount} posts", postIds.Count);

        // ❌ PROBLEMÁTICO: Uma query para cada update
        foreach (var postId in postIds)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        sw.Stop();
        _logger.LogWarning("🐌 CONCLUÍDO: Updates individuais em {Duration}ms - {PostCount} posts", 
            sw.ElapsedMilliseconds, postIds.Count);
    }

    /// <summary>
    /// SOLUÇÃO: Bulk update (mais eficiente)
    /// </summary>
    public async Task UpdatePostViewsBulk(List<int> postIds)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🚀 INICIANDO: Bulk update - {PostCount} posts", postIds.Count);

        // ✅ OTIMIZADO: Uma única transação para múltiplos updates
        var posts = await _context.Posts
            .Where(p => postIds.Contains(p.Id))
            .ToListAsync();

        foreach (var post in posts)
        {
            post.ViewCount++;
        }

        await _context.SaveChangesAsync();

        sw.Stop();
        _logger.LogInformation("🚀 CONCLUÍDO: Bulk update em {Duration}ms - {PostCount} posts", 
            sw.ElapsedMilliseconds, posts.Count);
    }

    // ==========================================
    // DEMONSTRAÇÕES DE TRACKING
    // ==========================================

    /// <summary>
    /// PROBLEMA: Consulta com tracking desnecessário
    /// </summary>
    public async Task<List<PostSummaryDto>> GetPostsWithTracking()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🐌 INICIANDO: Consulta com tracking");

        // ❌ PROBLEMÁTICO: Tracking overhead desnecessário para read-only
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Where(p => p.IsPublished)
            .Take(50)
            .ToListAsync();

        var result = posts.Select(p => new PostSummaryDto
        {
            Id = p.Id,
            Title = p.Title,
            Summary = p.Summary,
            CreatedAt = p.CreatedAt,
            AuthorName = p.Author.Name,
            CategoryName = p.Category?.Name,
            ViewCount = p.ViewCount,
            LikeCount = p.LikeCount
        }).ToList();

        sw.Stop();
        _logger.LogWarning("🐌 CONCLUÍDO: Consulta com tracking em {Duration}ms - {PostCount} posts", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    /// <summary>
    /// SOLUÇÃO: Consulta sem tracking para read-only
    /// </summary>
    public async Task<List<PostSummaryDto>> GetPostsWithoutTracking()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🚀 INICIANDO: Consulta sem tracking");

        // ✅ OTIMIZADO: AsNoTracking para consultas read-only
        var result = await _context.Posts
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .Take(50)
            .Select(p => new PostSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                Summary = p.Summary,
                CreatedAt = p.CreatedAt,
                AuthorName = p.Author.Name,
                CategoryName = p.Category != null ? p.Category.Name : null,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount
            })
            .ToListAsync();

        sw.Stop();
        _logger.LogInformation("🚀 CONCLUÍDO: Consulta sem tracking em {Duration}ms - {PostCount} posts", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    // ==========================================
    // DEMONSTRAÇÃO DE SPLIT QUERIES
    // ==========================================

    /// <summary>
    /// PROBLEMA: Cartesian explosion com múltiplos includes
    /// </summary>
    public async Task<List<PostDetailDto>> GetPostsWithCartesianExplosion()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🐌 INICIANDO: Consulta com Cartesian explosion");

        // ❌ PROBLEMÁTICO: JOINs múltiplos causam duplicação de dados
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Comments)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.IsPublished)
            .Take(10)
            .ToListAsync();

        var result = posts.Select(p => new PostDetailDto
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Summary = p.Summary,
            CreatedAt = p.CreatedAt,
            AuthorName = p.Author.Name,
            CategoryName = p.Category?.Name,
            Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList(),
            Comments = p.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                AuthorName = c.AuthorName,
                CreatedAt = c.CreatedAt
            }).ToList()
        }).ToList();

        sw.Stop();
        _logger.LogWarning("🐌 CONCLUÍDO: Cartesian explosion em {Duration}ms - {PostCount} posts", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    /// <summary>
    /// SOLUÇÃO: Split queries para evitar Cartesian explosion
    /// </summary>
    public async Task<List<PostDetailDto>> GetPostsWithSplitQueries()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🚀 INICIANDO: Consulta com Split Queries");

        // ✅ OTIMIZADO: Split queries evitam duplicação
        var posts = await _context.Posts
            .AsSplitQuery()
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Comments)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.IsPublished)
            .Take(10)
            .ToListAsync();

        var result = posts.Select(p => new PostDetailDto
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Summary = p.Summary,
            CreatedAt = p.CreatedAt,
            AuthorName = p.Author.Name,
            CategoryName = p.Category?.Name,
            Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList(),
            Comments = p.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                AuthorName = c.AuthorName,
                CreatedAt = c.CreatedAt
            }).ToList()
        }).ToList();

        sw.Stop();
        _logger.LogInformation("🚀 CONCLUÍDO: Split Queries em {Duration}ms - {PostCount} posts", 
            sw.ElapsedMilliseconds, result.Count);

        return result;
    }

    // ==========================================
    // MÉTODOS DE BENCHMARK COMPARATIVO
    // ==========================================

    public async Task<BenchmarkResult> RunPerformanceComparison()
    {
        var result = new BenchmarkResult
        {
            TestName = "Entity Framework Performance Comparison",
            Results = new List<PerformanceResult>()
        };

        // Test N+1 vs Optimized
        await TestMethod(result, "N+1 Problem", async () => await GetPostsWithN1Problem());
        await TestMethod(result, "Eager Loading", async () => await GetPostsWithEagerLoading());
        await TestMethod(result, "Projection", async () => await GetPostsWithProjection());

        // Test Tracking
        await TestMethod(result, "With Tracking", async () => await GetPostsWithTracking());
        await TestMethod(result, "No Tracking", async () => await GetPostsWithoutTracking());

        // Test Complex Queries
        await TestMethod(result, "Statistics Non-Optimized", async () => await GetBlogStatisticsNonOptimized());
        await TestMethod(result, "Statistics Optimized", async () => await GetBlogStatisticsOptimized());

        return result;
    }

    private async Task TestMethod<T>(BenchmarkResult benchmark, string methodName, Func<Task<T>> method)
    {
        var sw = Stopwatch.StartNew();
        var startMemory = GC.GetTotalMemory(false);

        try
        {
            var result = await method();
            sw.Stop();

            var endMemory = GC.GetTotalMemory(false);
            var memoryUsed = endMemory - startMemory;

            benchmark.Results.Add(new PerformanceResult
            {
                Operation = "Query Execution",
                Method = methodName,
                Duration = sw.Elapsed,
                MemoryUsed = memoryUsed,
                RecordsProcessed = result switch
                {
                    System.Collections.ICollection collection => collection.Count,
                    _ => 1
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar teste {MethodName}", methodName);
            benchmark.Results.Add(new PerformanceResult
            {
                Operation = "Query Execution",
                Method = methodName,
                Duration = sw.Elapsed,
                AdditionalInfo = new Dictionary<string, object> { ["Error"] = ex.Message }
            });
        }
    }
}
