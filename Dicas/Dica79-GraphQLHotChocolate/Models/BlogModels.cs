namespace Dica79.GraphQLHotChocolate.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public UserStatus Status { get; set; }
    public UserProfile? Profile { get; set; }
    public List<Post> Posts { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Website { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Location { get; set; }
    public User User { get; set; } = null!;
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public PostStatus Status { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public List<Comment> Comments { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
}

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public List<Comment> Replies { get; set; } = new();
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3b82f6";
    public DateTime CreatedAt { get; set; }
    public List<Post> Posts { get; set; } = new();
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}

public enum PostStatus
{
    Draft,
    Published,
    Archived
}

// Input Types for Mutations
public class CreateUserInput
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
}

public class CreatePostInput
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public List<string> TagNames { get; set; } = new();
}

public class CreateCommentInput
{
    public string Content { get; set; } = string.Empty;
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }
}

public class UpdatePostInput
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public PostStatus? Status { get; set; }
    public List<string>? TagNames { get; set; }
}

// Payload Types for Mutations
public class CreateUserPayload
{
    public User? User { get; set; }
    public List<UserError> Errors { get; set; } = new();
}

public class CreatePostPayload
{
    public Post? Post { get; set; }
    public List<UserError> Errors { get; set; } = new();
}

public class CreateCommentPayload
{
    public Comment? Comment { get; set; }
    public List<UserError> Errors { get; set; } = new();
}

public class UpdatePostPayload
{
    public Post? Post { get; set; }
    public List<UserError> Errors { get; set; } = new();
}

// Filter Types
public class PostFilterInput
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsPublished { get; set; }
    public int? AuthorId { get; set; }
    public List<string>? TagNames { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}

public class UserFilterInput
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public UserStatus? Status { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}

// Subscription Types
public class PostSubscriptionPayload
{
    public Post Post { get; set; } = null!;
    public string Action { get; set; } = string.Empty; // "CREATED", "UPDATED", "DELETED"
}

public class CommentSubscriptionPayload
{
    public Comment Comment { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public int PostId { get; set; }
}

// Additional missing types
public class BlogStatistics
{
    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public int TotalComments { get; set; }
    public int TotalTags { get; set; }
    public int PublishedPosts { get; set; }
    public int DraftPosts { get; set; }
}

public class PostCountByStatus
{
    public PostStatus Status { get; set; }
    public int Count { get; set; }
}

public class UpdateUserInput
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UpdateUserPayload
{
    public User? User { get; set; }
    public List<UserError> Errors { get; set; } = new();
}

public class DeletePostPayload
{
    public bool Success { get; set; }
    public List<UserError> Errors { get; set; } = new();
}

public class UserError
{
    public string Message { get; set; }
    public string Code { get; set; }

    public UserError(string message, string code)
    {
        Message = message;
        Code = code;
    }
}
