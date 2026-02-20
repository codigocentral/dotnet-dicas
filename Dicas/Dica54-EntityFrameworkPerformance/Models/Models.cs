using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Dica54_EntityFrameworkPerformance.Models;

// ==========================================
// ENTIDADES DO DOMÍNIO
// ==========================================

public class Blog
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Propriedades calculadas
    public int PostCount { get; set; }
    public int ViewCount { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
}

public class Post
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    
    public bool IsPublished { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    
    // Foreign Keys
    public int BlogId { get; set; }
    public int AuthorId { get; set; }
    public int? CategoryId { get; set; }
    
    // Navigation Properties
    public virtual Blog Blog { get; set; } = null!;
    public virtual Author Author { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}

public class Author
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Estatísticas
    public int PostCount { get; set; } = 0;
    public int TotalViews { get; set; } = 0;
    public int TotalLikes { get; set; } = 0;
    
    // Navigation Properties
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#007bff";
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}

public class Comment
{
    public int Id { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;
    
    [MaxLength(150)]
    public string AuthorEmail { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; } = false;
    public int LikeCount { get; set; } = 0;
    
    // Foreign Key
    public int PostId { get; set; }
    
    // Navigation Property
    public virtual Post Post { get; set; } = null!;
}

public class Tag
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#6c757d";
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public virtual ICollection<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
}

// ==========================================
// TABELAS DE RELACIONAMENTO (Many-to-Many)
// ==========================================

public class PostTag
{
    public int PostId { get; set; }
    public int TagId { get; set; }
    
    public virtual Post Post { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}

public class BlogTag
{
    public int BlogId { get; set; }
    public int TagId { get; set; }
    
    public virtual Blog Blog { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}

// ==========================================
// DTOs PARA PERFORMANCE
// ==========================================

public class PostSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}

public class BlogStatisticsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public int TotalViews { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
    public DateTime LastPostDate { get; set; }
}

public class AuthorStatisticsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public int TotalViews { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
    public DateTime LastPostDate { get; set; }
    public List<string> TopCategories { get; set; } = new();
}

public class PostDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    
    public string BlogTitle { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    
    public List<string> Tags { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
}

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int LikeCount { get; set; }
    public bool IsApproved { get; set; }
}

// ==========================================
// MODELOS PARA CRIAÇÃO E ATUALIZAÇÃO
// ==========================================

public class CreateBlogRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public List<int> TagIds { get; set; } = new();
}

public class CreatePostRequest
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;
    
    public int BlogId { get; set; }
    public int AuthorId { get; set; }
    public int? CategoryId { get; set; }
    
    public bool IsPublished { get; set; } = false;
    public List<int> TagIds { get; set; } = new();
}

public class UpdatePostRequest
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;
    
    public int? CategoryId { get; set; }
    public bool IsPublished { get; set; }
    
    public List<int> TagIds { get; set; } = new();
}

// ==========================================
// RESULTADOS DE PERFORMANCE
// ==========================================

public class PerformanceResult
{
    public string Operation { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public long MemoryUsed { get; set; }
    public int RecordsProcessed { get; set; }
    public string SqlQuery { get; set; } = string.Empty;
    public bool HasN1Problem { get; set; }
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
}

public class BenchmarkResult
{
    public string TestName { get; set; } = string.Empty;
    public List<PerformanceResult> Results { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
