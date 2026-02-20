using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Dica54_EntityFrameworkPerformance.Data;
using Dica54_EntityFrameworkPerformance.Models;

namespace Dica54_EntityFrameworkPerformance.Services;

/// <summary>
/// Serviço para seed e geração de dados de teste
/// </summary>
public class DataSeedService
{
    private readonly BlogContext _context;
    private readonly ILogger<DataSeedService> _logger;
    private readonly Random _random = new();

    public DataSeedService(BlogContext context, ILogger<DataSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🌱 Iniciando seed de dados...");

        await _context.Database.EnsureCreatedAsync();

        // Verificar se já existem dados
        if (await _context.Posts.AnyAsync())
        {
            _logger.LogInformation("✅ Dados já existem, pulando seed");
            return;
        }

        await SeedBlogsAsync();
        await SeedPostsAsync();
        await SeedCommentsAsync();
        await SeedTagRelationsAsync();

        sw.Stop();
        _logger.LogInformation("🌱 Seed concluído em {Duration}ms", sw.ElapsedMilliseconds);
    }

    private async Task SeedBlogsAsync()
    {
        var blogs = new[]
        {
            new Blog
            {
                Title = "Tech Blog Brasil",
                Description = "Blog sobre tecnologia e desenvolvimento",
                CreatedAt = DateTime.UtcNow.AddMonths(-12),
                ViewCount = _random.Next(50000, 100000)
            },
            new Blog
            {
                Title = "Código Limpo",
                Description = "Melhores práticas em programação",
                CreatedAt = DateTime.UtcNow.AddMonths(-10),
                ViewCount = _random.Next(30000, 80000)
            },
            new Blog
            {
                Title = "Performance Matters",
                Description = "Otimização e performance em .NET",
                CreatedAt = DateTime.UtcNow.AddMonths(-8),
                ViewCount = _random.Next(20000, 60000)
            },
            new Blog
            {
                Title = "Database Pro",
                Description = "Banco de dados e Entity Framework",
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                ViewCount = _random.Next(15000, 45000)
            },
            new Blog
            {
                Title = "Modern .NET",
                Description = "Recursos modernos do .NET",
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                ViewCount = _random.Next(10000, 30000)
            }
        };

        _context.Blogs.AddRange(blogs);
        await _context.SaveChangesAsync();
        _logger.LogInformation("✅ {Count} blogs criados", blogs.Length);
    }

    private async Task SeedPostsAsync()
    {
        var blogs = await _context.Blogs.ToListAsync();
        var authors = await _context.Authors.ToListAsync();
        var categories = await _context.Categories.ToListAsync();

        var posts = new List<Post>();

        var postTitles = new[]
        {
            "Entity Framework Core Performance Tips",
            "LINQ: Best Practices for Better Performance",
            "Async/Await Patterns in .NET",
            "Memory Management in C#",
            "Database Optimization Strategies",
            "Clean Architecture with .NET",
            "Microservices Communication Patterns",
            "Testing Strategies for .NET Applications",
            "Security Best Practices in Web APIs",
            "Docker and .NET: A Complete Guide",
            "CI/CD for .NET Applications",
            "Monitoring and Logging in Production",
            "Performance Profiling Tools",
            "Code Quality and Static Analysis",
            "Design Patterns in Modern C#",
            "API Design Guidelines",
            "Error Handling Strategies",
            "Caching Techniques for Web Apps",
            "Database Migration Strategies",
            "Scalability Patterns for .NET"
        };

        var contentTemplate = @"
Este é um post sobre {0}. 

## Introdução
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.

## Desenvolvimento
Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.

### Exemplo de Código
```csharp
public class ExemploClasse
{{
    public string Propriedade {{ get; set; }}
    
    public void Metodo()
    {{
        // Implementação aqui
    }}
}}
```

## Conclusão
Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
        ";

        // Criar 500 posts para simular volume real
        for (int i = 0; i < 500; i++)
        {
            var blog = blogs[_random.Next(blogs.Count)];
            var author = authors[_random.Next(authors.Count)];
            var category = categories[_random.Next(categories.Count)];
            var title = postTitles[_random.Next(postTitles.Length)];
            
            var createdAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365));
            var isPublished = _random.NextDouble() > 0.1; // 90% publicados
            
            posts.Add(new Post
            {
                Title = $"{title} - Parte {i + 1}",
                Content = string.Format(contentTemplate, title),
                Summary = $"Este post aborda {title.ToLower()} com exemplos práticos e dicas de implementação.",
                BlogId = blog.Id,
                AuthorId = author.Id,
                CategoryId = category.Id,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddDays(_random.Next(0, 30)),
                PublishedAt = isPublished ? createdAt.AddDays(_random.Next(0, 7)) : null,
                IsPublished = isPublished,
                ViewCount = _random.Next(100, 10000),
                LikeCount = _random.Next(5, 500)
            });

            // Commit em lotes para evitar problemas de memória
            if (posts.Count >= 100)
            {
                _context.Posts.AddRange(posts);
                await _context.SaveChangesAsync();
                posts.Clear();
                _logger.LogInformation("📝 {Count} posts criados...", i + 1);
            }
        }

        if (posts.Count > 0)
        {
            _context.Posts.AddRange(posts);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("✅ 500 posts criados");
    }

    private async Task SeedCommentsAsync()
    {
        var posts = await _context.Posts.Take(200).ToListAsync(); // Comentários apenas nos primeiros 200 posts

        var commentAuthors = new[]
        {
            "João Silva", "Maria Santos", "Pedro Costa", "Ana Lima", "Carlos Oliveira",
            "Fernanda Souza", "Ricardo Almeida", "Patrícia Ferreira", "Lucas Rodrigues", "Camila Pereira"
        };

        var commentTexts = new[]
        {
            "Excelente post! Muito esclarecedor.",
            "Obrigado pelas dicas, foi muito útil.",
            "Poderia dar mais exemplos práticos?",
            "Implementei essa solução e funcionou perfeitamente.",
            "Muito bom! Já compartilhei com a equipe.",
            "Tenho uma dúvida sobre o exemplo apresentado...",
            "Fantástico! Aguardo mais posts como este.",
            "Muito didático e bem explicado.",
            "Exatamente o que eu estava procurando!",
            "Parabéns pelo conteúdo de qualidade."
        };

        var comments = new List<Comment>();

        foreach (var post in posts)
        {
            var commentCount = _random.Next(0, 15); // 0 a 15 comentários por post

            for (int i = 0; i < commentCount; i++)
            {
                comments.Add(new Comment
                {
                    PostId = post.Id,
                    Content = commentTexts[_random.Next(commentTexts.Length)],
                    AuthorName = commentAuthors[_random.Next(commentAuthors.Length)],
                    AuthorEmail = $"user{_random.Next(1, 1000)}@example.com",
                    CreatedAt = post.CreatedAt.AddDays(_random.Next(0, 30)),
                    IsApproved = _random.NextDouble() > 0.2, // 80% aprovados
                    LikeCount = _random.Next(0, 50)
                });
            }

            // Commit em lotes
            if (comments.Count >= 500)
            {
                _context.Comments.AddRange(comments);
                await _context.SaveChangesAsync();
                comments.Clear();
            }
        }

        if (comments.Count > 0)
        {
            _context.Comments.AddRange(comments);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("✅ Comentários criados");
    }

    private async Task SeedTagRelationsAsync()
    {
        var posts = await _context.Posts.ToListAsync();
        var blogs = await _context.Blogs.ToListAsync();
        var tags = await _context.Tags.ToListAsync();

        var postTags = new List<PostTag>();
        var blogTags = new List<BlogTag>();

        // Associar tags aos posts
        foreach (var post in posts)
        {
            var tagCount = _random.Next(1, 4); // 1 a 3 tags por post
            var selectedTags = tags.OrderBy(x => _random.Next()).Take(tagCount);

            foreach (var tag in selectedTags)
            {
                postTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
            }
        }

        // Associar tags aos blogs
        foreach (var blog in blogs)
        {
            var tagCount = _random.Next(2, 6); // 2 a 5 tags por blog
            var selectedTags = tags.OrderBy(x => _random.Next()).Take(tagCount);

            foreach (var tag in selectedTags)
            {
                blogTags.Add(new BlogTag { BlogId = blog.Id, TagId = tag.Id });
            }
        }

        _context.PostTags.AddRange(postTags);
        _context.BlogTags.AddRange(blogTags);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Relacionamentos de tags criados: {PostTags} PostTags, {BlogTags} BlogTags", 
            postTags.Count, blogTags.Count);
    }

    public async Task<string> GetDatabaseStatistics()
    {
        var blogCount = await _context.Blogs.CountAsync();
        var postCount = await _context.Posts.CountAsync();
        var authorCount = await _context.Authors.CountAsync();
        var categoryCount = await _context.Categories.CountAsync();
        var commentCount = await _context.Comments.CountAsync();
        var tagCount = await _context.Tags.CountAsync();
        var postTagCount = await _context.PostTags.CountAsync();
        var blogTagCount = await _context.BlogTags.CountAsync();

        return $@"
📊 Estatísticas do Banco de Dados:
┌─────────────────┬─────────┐
│ Tabela          │ Registros │
├─────────────────┼─────────┤
│ Blogs           │ {blogCount,7} │
│ Posts           │ {postCount,7} │
│ Authors         │ {authorCount,7} │
│ Categories      │ {categoryCount,7} │
│ Comments        │ {commentCount,7} │
│ Tags            │ {tagCount,7} │
│ PostTags        │ {postTagCount,7} │
│ BlogTags        │ {blogTagCount,7} │
└─────────────────┴─────────┘

💡 Total de registros: {blogCount + postCount + authorCount + categoryCount + commentCount + tagCount + postTagCount + blogTagCount}";
    }
}
