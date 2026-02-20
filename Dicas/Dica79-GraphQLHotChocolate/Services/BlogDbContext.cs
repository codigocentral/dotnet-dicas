using Microsoft.EntityFrameworkCore;
using Dica79.GraphQLHotChocolate.Models;
using Bogus;
using BlogTag = Dica79.GraphQLHotChocolate.Models.Tag;

namespace Dica79.GraphQLHotChocolate.Services;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> Profiles { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<BlogTag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasOne(e => e.Profile)
                  .WithOne(p => p.User)
                  .HasForeignKey<UserProfile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Posts)
                  .WithOne(p => p.Author)
                  .HasForeignKey(p => p.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Comments)
                  .WithOne(c => c.Author)
                  .HasForeignKey(c => c.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(100);
        });

        // Post configuration
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            
            entity.HasMany(e => e.Comments)
                  .WithOne(c => c.Post)
                  .HasForeignKey(c => c.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Tags)
                  .WithMany(t => t.Posts)
                  .UsingEntity(j => j.ToTable("PostTags"));
        });

        // Comment configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            
            entity.HasOne(e => e.ParentComment)
                  .WithMany(c => c.Replies)
                  .HasForeignKey(e => e.ParentCommentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Tag configuration
        modelBuilder.Entity<BlogTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}

public class DatabaseSeeder
{
    private readonly BlogDbContext _context;

    public DatabaseSeeder(BlogDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
            return; // Already seeded

        await SeedUsersAsync();
        await SeedTagsAsync();
        await SeedPostsAsync();
        await SeedCommentsAsync();
        
        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Username, f => f.Internet.UserName())
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.LastName, f => f.Person.LastName)
            .RuleFor(u => u.PasswordHash, f => "hashed_password")
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(2))
            .RuleFor(u => u.UpdatedAt, f => f.Date.Recent(30))
            .RuleFor(u => u.LastLoginAt, f => f.Date.Recent(30))
            .RuleFor(u => u.Status, f => f.PickRandom<UserStatus>());

        var profileFaker = new Faker<UserProfile>()
            .RuleFor(p => p.Bio, f => f.Lorem.Sentence(10))
            .RuleFor(p => p.AvatarUrl, f => f.Internet.Avatar())
            .RuleFor(p => p.Website, f => f.Internet.Url())
            .RuleFor(p => p.BirthDate, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
            .RuleFor(p => p.Location, f => f.Address.City());

        var users = userFaker.Generate(20);
        
        foreach (var user in users)
        {
            user.Profile = profileFaker.Generate();
            user.Profile.UserId = user.Id;
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTagsAsync()
    {
        var colors = new[] { "#ef4444", "#f97316", "#eab308", "#22c55e", "#06b6d4", "#3b82f6", "#8b5cf6", "#ec4899" };
        
        var tagNames = new[]
        {
            "CSharp", "DotNet", "GraphQL", "HotChocolate", "EntityFramework",
            "WebAPI", "Architecture", "Performance", "Security", "Testing",
            "Azure", "Docker", "Microservices", "CleanCode", "SOLID"
        };

        var tags = tagNames.Select((name, index) => new BlogTag
        {
            Name = name,
            Color = colors[index % colors.Length],
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365))
        }).ToList();

        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();
    }

    private async Task SeedPostsAsync()
    {
        var users = await _context.Users.ToListAsync();
        var tags = await _context.Tags.ToListAsync();

        var postFaker = new Faker<Post>()
            .RuleFor(p => p.Title, f => f.Lorem.Sentence(5, 10).TrimEnd('.'))
            .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(3, 8))
            .RuleFor(p => p.Summary, f => f.Lorem.Paragraph())
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent(30))
            .RuleFor(p => p.Status, f => f.PickRandom<PostStatus>())
            .RuleFor(p => p.ViewCount, f => f.Random.Int(0, 10000))
            .RuleFor(p => p.LikeCount, f => f.Random.Int(0, 500))
            .RuleFor(p => p.AuthorId, f => f.PickRandom(users).Id);

        var posts = postFaker.Generate(50);

        foreach (var post in posts)
        {
            // Assign random tags to each post
            var randomTagCount = Random.Shared.Next(1, 5);
            var selectedTags = tags.OrderBy(x => Random.Shared.Next()).Take(randomTagCount).ToList();
            post.Tags = selectedTags;
        }

        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();
    }

    private async Task SeedCommentsAsync()
    {
        var users = await _context.Users.ToListAsync();
        var posts = await _context.Posts.ToListAsync();

        var commentFaker = new Faker<Comment>()
            .RuleFor(c => c.Content, f => f.Lorem.Paragraph())
            .RuleFor(c => c.CreatedAt, f => f.Date.Recent(30))
            .RuleFor(c => c.UpdatedAt, f => f.Date.Recent(15))
            .RuleFor(c => c.AuthorId, f => f.PickRandom(users).Id)
            .RuleFor(c => c.PostId, f => f.PickRandom(posts).Id);

        // Generate top-level comments
        var topLevelComments = commentFaker.Generate(150);
        await _context.Comments.AddRangeAsync(topLevelComments);
        await _context.SaveChangesAsync();

        // Generate replies to some comments
        var parentComments = topLevelComments.Take(50).ToList();
        var replyFaker = new Faker<Comment>()
            .RuleFor(c => c.Content, f => f.Lorem.Sentence(5, 15))
            .RuleFor(c => c.CreatedAt, f => f.Date.Recent(20))
            .RuleFor(c => c.AuthorId, f => f.PickRandom(users).Id)
            .RuleFor(c => c.PostId, f => f.PickRandom(parentComments).PostId)
            .RuleFor(c => c.ParentCommentId, f => f.PickRandom(parentComments).Id);

        var replies = replyFaker.Generate(75);
        await _context.Comments.AddRangeAsync(replies);
        await _context.SaveChangesAsync();
    }
}
