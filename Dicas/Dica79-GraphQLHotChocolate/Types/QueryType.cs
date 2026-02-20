using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Dica79.GraphQLHotChocolate.Models;
using Dica79.GraphQLHotChocolate.Services;
using BlogTag = Dica79.GraphQLHotChocolate.Models.Tag;

namespace Dica79.GraphQLHotChocolate.Types;

[QueryType]
public class Query
{
    /// <summary>
    /// Get all users with filtering, sorting and pagination
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers(BlogDbContext context) =>
        context.Users;

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(
        int id,
        BlogDbContext context) =>
        await context.Users
            .Include(u => u.Posts)
            .FirstOrDefaultAsync(u => u.Id == id);

    /// <summary>
    /// Get all posts with filtering, sorting and pagination
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> GetPosts(BlogDbContext context) =>
        context.Posts;

    /// <summary>
    /// Get post by ID with comments
    /// </summary>
    public async Task<Post?> GetPostByIdAsync(
        int id,
        BlogDbContext context) =>
        await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

    /// <summary>
    /// Get posts by user ID
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> GetPostsByUserId(
        int userId,
        BlogDbContext context) =>
        context.Posts.Where(p => p.AuthorId == userId);

    /// <summary>
    /// Get all comments
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Comment> GetComments(BlogDbContext context) =>
        context.Comments;

    /// <summary>
    /// Get comments by post ID
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Comment> GetCommentsByPostId(
        int postId,
        BlogDbContext context) =>
        context.Comments.Where(c => c.PostId == postId);

    /// <summary>
    /// Get all tags
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<BlogTag> GetTags(BlogDbContext context) =>
        context.Tags;

    /// <summary>
    /// Get posts by tag name
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> GetPostsByTag(
        string tagName,
        BlogDbContext context) =>
        context.Posts
            .Where(p => p.Tags.Any(t => t.Name == tagName));

    /// <summary>
    /// Search posts by title or content
    /// </summary>
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> SearchPosts(
        string searchTerm,
        BlogDbContext context) =>
        context.Posts
            .Where(p => p.Title.Contains(searchTerm) || 
                       p.Content.Contains(searchTerm));

    /// <summary>
    /// Get blog statistics
    /// </summary>
    public async Task<BlogStatistics> GetBlogStatisticsAsync(BlogDbContext context)
    {
        var totalUsers = await context.Users.CountAsync();
        var totalPosts = await context.Posts.CountAsync();
        var totalComments = await context.Comments.CountAsync();
        var totalTags = await context.Tags.CountAsync();
        var publishedPosts = await context.Posts.CountAsync(p => p.Status == PostStatus.Published);
        var draftPosts = await context.Posts.CountAsync(p => p.Status == PostStatus.Draft);

        return new BlogStatistics
        {
            TotalUsers = totalUsers,
            TotalPosts = totalPosts,
            TotalComments = totalComments,
            TotalTags = totalTags,
            PublishedPosts = publishedPosts,
            DraftPosts = draftPosts
        };
    }

    /// <summary>
    /// Get recent posts (last 10)
    /// </summary>
    [UseProjection]
    public IQueryable<Post> GetRecentPosts(BlogDbContext context) =>
        context.Posts
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10);

    /// <summary>
    /// Get popular posts (most commented)
    /// </summary>
    [UseProjection]
    public IQueryable<Post> GetPopularPosts(BlogDbContext context) =>
        context.Posts
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.Comments.Count)
            .Take(10);

    /// <summary>
    /// Get posts count by status
    /// </summary>
    public async Task<List<PostCountByStatus>> GetPostCountByStatusAsync(BlogDbContext context)
    {
        return await context.Posts
            .GroupBy(p => p.Status)
            .Select(g => new PostCountByStatus
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }
}
