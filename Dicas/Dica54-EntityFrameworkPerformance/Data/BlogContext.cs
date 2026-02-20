using Microsoft.EntityFrameworkCore;
using Dica54_EntityFrameworkPerformance.Models;

namespace Dica54_EntityFrameworkPerformance.Data;

public class BlogContext : DbContext
{
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<BlogTag> BlogTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // CONFIGURAÇÕES DE ENTIDADES
        // ==========================================

        // Blog Configuration
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Índices para performance
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.IsActive, e.CreatedAt });
        });

        // Post Configuration
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Relacionamentos
            entity.HasOne(e => e.Blog)
                  .WithMany(b => b.Posts)
                  .HasForeignKey(e => e.BlogId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Author)
                  .WithMany(a => a.Posts)
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Posts)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // Índices críticos para performance
            entity.HasIndex(e => e.BlogId);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.PublishedAt);
            entity.HasIndex(e => new { e.IsPublished, e.CreatedAt });
            entity.HasIndex(e => new { e.BlogId, e.IsPublished });
            entity.HasIndex(e => new { e.AuthorId, e.IsPublished });
            entity.HasIndex(e => e.Title); // Para busca textual
        });

        // Author Configuration
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Constraints únicos
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Índices
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
        });

        // Category Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Color).HasDefaultValue("#007bff");
            
            // Constraint único
            entity.HasIndex(e => e.Name).IsUnique();
            
            // Índices
            entity.HasIndex(e => e.IsActive);
        });

        // Comment Configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.AuthorName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AuthorEmail).HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Relacionamento
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Índices
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.IsApproved);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.PostId, e.IsApproved });
        });

        // Tag Configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).HasDefaultValue("#6c757d");
            
            // Constraint único
            entity.HasIndex(e => e.Name).IsUnique();
            
            // Índices
            entity.HasIndex(e => e.IsActive);
        });

        // ==========================================
        // RELACIONAMENTOS MANY-TO-MANY
        // ==========================================

        // PostTag (Many-to-Many entre Post e Tag)
        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.HasKey(e => new { e.PostId, e.TagId });
            
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.PostTags)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.PostTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Índices para performance
            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.TagId);
        });

        // BlogTag (Many-to-Many entre Blog e Tag)
        modelBuilder.Entity<BlogTag>(entity =>
        {
            entity.HasKey(e => new { e.BlogId, e.TagId });
            
            entity.HasOne(e => e.Blog)
                  .WithMany(b => b.BlogTags)
                  .HasForeignKey(e => e.BlogId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.BlogTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Índices para performance
            entity.HasIndex(e => e.BlogId);
            entity.HasIndex(e => e.TagId);
        });

        // ==========================================
        // SEEDS INICIAIS
        // ==========================================
        
        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Tecnologia", Description = "Posts sobre tecnologia", Color = "#007bff" },
            new Category { Id = 2, Name = "Programação", Description = "Posts sobre programação", Color = "#28a745" },
            new Category { Id = 3, Name = "Tutorial", Description = "Tutoriais passo a passo", Color = "#ffc107" },
            new Category { Id = 4, Name = "Dicas", Description = "Dicas e truques", Color = "#17a2b8" },
            new Category { Id = 5, Name = "Opinião", Description = "Artigos de opinião", Color = "#6f42c1" }
        );
        
        // Tags
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "C#", Color = "#239120" },
            new Tag { Id = 2, Name = ".NET", Color = "#512bd4" },
            new Tag { Id = 3, Name = "Entity Framework", Color = "#0078d4" },
            new Tag { Id = 4, Name = "Performance", Color = "#ff6b6b" },
            new Tag { Id = 5, Name = "LINQ", Color = "#4ecdc4" },
            new Tag { Id = 6, Name = "SQL", Color = "#45b7d1" },
            new Tag { Id = 7, Name = "Database", Color = "#96ceb4" },
            new Tag { Id = 8, Name = "Optimization", Color = "#ffeaa7" },
            new Tag { Id = 9, Name = "Best Practices", Color = "#fab1a0" },
            new Tag { Id = 10, Name = "Tutorial", Color = "#fd79a8" }
        );
        
        // Authors
        modelBuilder.Entity<Author>().HasData(
            new Author 
            { 
                Id = 1, 
                Name = "João Silva", 
                Email = "joao@example.com", 
                Bio = "Desenvolvedor .NET com 10 anos de experiência",
                CreatedAt = DateTime.UtcNow.AddMonths(-12)
            },
            new Author 
            { 
                Id = 2, 
                Name = "Maria Santos", 
                Email = "maria@example.com", 
                Bio = "Arquiteta de software especializada em performance",
                CreatedAt = DateTime.UtcNow.AddMonths(-8)
            },
            new Author 
            { 
                Id = 3, 
                Name = "Pedro Costa", 
                Email = "pedro@example.com", 
                Bio = "DBA e especialista em Entity Framework",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            }
        );
    }

    // ==========================================
    // CONFIGURAÇÕES DE PERFORMANCE
    // ==========================================
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configurações para melhor performance em development
        optionsBuilder.EnableSensitiveDataLogging()
                     .EnableDetailedErrors();
    }
}

// ==========================================
// EXTENSÕES PARA CONSULTAS OTIMIZADAS
// ==========================================

public static class BlogContextExtensions
{
    /// <summary>
    /// Query otimizada para buscar posts com includes necessários
    /// </summary>
    public static IQueryable<Post> WithOptimizedIncludes(this IQueryable<Post> posts)
    {
        return posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Blog);
    }
    
    /// <summary>
    /// Query otimizada para posts publicados
    /// </summary>
    public static IQueryable<Post> PublishedOnly(this IQueryable<Post> posts)
    {
        return posts.Where(p => p.IsPublished && p.PublishedAt != null);
    }
    
    /// <summary>
    /// Query otimizada para posts recentes
    /// </summary>
    public static IQueryable<Post> Recent(this IQueryable<Post> posts, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return posts.Where(p => p.CreatedAt >= cutoffDate);
    }
    
    /// <summary>
    /// Query otimizada para busca por título
    /// </summary>
    public static IQueryable<Post> SearchByTitle(this IQueryable<Post> posts, string searchTerm)
    {
        return posts.Where(p => p.Title.Contains(searchTerm));
    }
    
    /// <summary>
    /// Query com paginação otimizada
    /// </summary>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize)
    {
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}
