using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using HotChocolate.Subscriptions;
using FluentValidation;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Dica79.GraphQLHotChocolate.Models;
using Dica79.GraphQLHotChocolate.Services;
using BlogTag = Dica79.GraphQLHotChocolate.Models.Tag;

namespace Dica79.GraphQLHotChocolate.Types;

[MutationType]
public class Mutation
{
    /// <summary>
    /// Create a new user
    /// </summary>
    public async Task<CreateUserPayload> CreateUserAsync(
        CreateUserInput input,
        BlogDbContext context,
        IValidator<CreateUserInput> validator,
        ITopicEventSender eventSender)
    {
        // Validate input
        var validationResult = await validator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            return new CreateUserPayload
            {
                User = null,
                Errors = validationResult.Errors.Select(e => new UserError(e.ErrorMessage, "VALIDATION_ERROR")).ToList()
            };
        }

        // Check if username already exists
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == input.Username);
        if (existingUser != null)
        {
            return new CreateUserPayload
            {
                User = null,
                Errors = new List<UserError> { new UserError("Username already exists", "USERNAME_EXISTS") }
            };
        }

        // Create new user
        var user = new User
        {
            Username = input.Username,
            Email = input.Email,
            PasswordHash = HashPassword(input.Password), // In production, use proper password hashing
            FirstName = input.FirstName,
            LastName = input.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Profile = new UserProfile
            {
                Bio = input.Bio,
                Website = input.Website,
                Location = input.Location,
                AvatarUrl = input.AvatarUrl
            }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Send subscription event
        await eventSender.SendAsync(nameof(Subscription.OnUserCreated), user);

        return new CreateUserPayload
        {
            User = user,
            Errors = new List<UserError>()
        };
    }

    /// <summary>
    /// Update user information
    /// </summary>
    public async Task<UpdateUserPayload> UpdateUserAsync(
        UpdateUserInput input,
        BlogDbContext context,
        IValidator<UpdateUserInput> validator,
        ITopicEventSender eventSender)
    {
        // Validate input
        var validationResult = await validator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            return new UpdateUserPayload
            {
                User = null,
                Errors = validationResult.Errors.Select(e => new UserError(e.ErrorMessage, "VALIDATION_ERROR")).ToList()
            };
        }

        // Find user
        var user = await context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == input.Id);
        
        if (user == null)
        {
            return new UpdateUserPayload
            {
                User = null,
                Errors = new List<UserError> { new UserError("User not found", "USER_NOT_FOUND") }
            };
        }

        // Update user
        user.Email = input.Email ?? user.Email;
        user.FirstName = input.FirstName ?? user.FirstName;
        user.LastName = input.LastName ?? user.LastName;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.Profile != null)
        {
            user.Profile.Bio = input.Bio ?? user.Profile.Bio;
            user.Profile.Website = input.Website ?? user.Profile.Website;
            user.Profile.Location = input.Location ?? user.Profile.Location;
            user.Profile.AvatarUrl = input.AvatarUrl ?? user.Profile.AvatarUrl;
        }

        await context.SaveChangesAsync();

        // Send subscription event
        await eventSender.SendAsync(nameof(Subscription.OnUserUpdated), user);

        return new UpdateUserPayload
        {
            User = user,
            Errors = new List<UserError>()
        };
    }

    /// <summary>
    /// Create a new blog post
    /// </summary>
    public async Task<CreatePostPayload> CreatePostAsync(
        CreatePostInput input,
        BlogDbContext context,
        IValidator<CreatePostInput> validator,
        ITopicEventSender eventSender,
        ClaimsPrincipal claimsPrincipal)
    {
        // Validate input
        var validationResult = await validator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            return new CreatePostPayload
            {
                Post = null,
                Errors = validationResult.Errors.Select(e => new UserError(e.ErrorMessage, "VALIDATION_ERROR")).ToList()
            };
        }

        // Get current user from claims
        var userIdClaim = claimsPrincipal.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var authorId))
        {
            return new CreatePostPayload
            {
                Post = null,
                Errors = new List<UserError> { new UserError("Authentication required", "AUTHENTICATION_REQUIRED") }
            };
        }

        // Verify author exists
        var author = await context.Users.FindAsync(authorId);
        if (author == null)
        {
            return new CreatePostPayload
            {
                Post = null,
                Errors = new List<UserError> { new UserError("Author not found", "AUTHOR_NOT_FOUND") }
            };
        }

        // Create post
        var post = new Post
        {
            Title = input.Title,
            Content = input.Content,
            Summary = input.Summary,
            AuthorId = authorId,
            Status = input.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Handle tags
        if (input.TagNames?.Any() == true)
        {
            var existingTags = await context.Tags
                .Where(t => input.TagNames.Contains(t.Name))
                .ToListAsync();

            var newTagNames = input.TagNames.Except(existingTags.Select(t => t.Name)).ToList();
            var newTags = newTagNames.Select(name => new BlogTag { Name = name }).ToList();

            context.Tags.AddRange(newTags);
            await context.SaveChangesAsync();

            post.Tags = existingTags.Concat(newTags).ToList();
        }

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Send subscription event
        await eventSender.SendAsync(nameof(Subscription.OnPostCreated), post);

        return new CreatePostPayload
        {
            Post = post,
            Errors = new List<UserError>()
        };
    }

    /// <summary>
    /// Update a blog post
    /// </summary>
    public async Task<UpdatePostPayload> UpdatePostAsync(
        UpdatePostInput input,
        BlogDbContext context,
        IValidator<UpdatePostInput> validator,
        ITopicEventSender eventSender,
        ClaimsPrincipal claimsPrincipal)
    {
        // Validate input
        var validationResult = await validator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            return new UpdatePostPayload
            {
                Post = null,
                Errors = validationResult.Errors.Select(e => new UserError(e.ErrorMessage, "VALIDATION_ERROR")).ToList()
            };
        }

        // Find post
        var post = await context.Posts
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == input.Id);

        if (post == null)
        {
            return new UpdatePostPayload
            {
                Post = null,
                Errors = new List<UserError> { new UserError("Post not found", "POST_NOT_FOUND") }
            };
        }

        // Check authorization (user can only edit their own posts)
        var userIdClaim = claimsPrincipal.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || post.AuthorId != userId)
        {
            return new UpdatePostPayload
            {
                Post = null,
                Errors = new List<UserError> { new UserError("Unauthorized", "UNAUTHORIZED") }
            };
        }

        // Update post
        post.Title = input.Title ?? post.Title;
        post.Content = input.Content ?? post.Content;
        post.Summary = input.Summary ?? post.Summary;
        post.Status = input.Status ?? post.Status;
        post.UpdatedAt = DateTime.UtcNow;

        // Handle tags update
        if (input.TagNames != null)
        {
            // Remove existing tags
            post.Tags.Clear();

            // Add new tags
            if (input.TagNames.Any())
            {
                var existingTags = await context.Tags
                    .Where(t => input.TagNames.Contains(t.Name))
                    .ToListAsync();

                var newTagNames = input.TagNames.Except(existingTags.Select(t => t.Name)).ToList();
                var newTags = newTagNames.Select(name => new BlogTag { Name = name }).ToList();

                context.Tags.AddRange(newTags);
                await context.SaveChangesAsync();

                post.Tags = existingTags.Concat(newTags).ToList();
            }
        }

        await context.SaveChangesAsync();

        // Send subscription event
        await eventSender.SendAsync(nameof(Subscription.OnPostUpdated), post);

        return new UpdatePostPayload
        {
            Post = post,
            Errors = new List<UserError>()
        };
    }

    /// <summary>
    /// Delete a blog post
    /// </summary>
    public async Task<DeletePostPayload> DeletePostAsync(
        int id,
        BlogDbContext context,
        ITopicEventSender eventSender,
        ClaimsPrincipal claimsPrincipal)
    {
        // Find post
        var post = await context.Posts.FindAsync(id);
        if (post == null)
        {
            return new DeletePostPayload
            {
                Success = false,
                Errors = new List<UserError> { new UserError("Post not found", "POST_NOT_FOUND") }
            };
        }

        // Check authorization
        var userIdClaim = claimsPrincipal.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId) || post.AuthorId != userId)
        {
            return new DeletePostPayload
            {
                Success = false,
                Errors = new List<UserError> { new UserError("Unauthorized", "UNAUTHORIZED") }
            };
        }

        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        // Send subscription event
        await eventSender.SendAsync(nameof(Subscription.OnPostDeleted), id);

        return new DeletePostPayload
        {
            Success = true,
            Errors = new List<UserError>()
        };
    }

    /// <summary>
    /// Create a comment on a post
    /// </summary>
    public async Task<CreateCommentPayload> CreateCommentAsync(
        CreateCommentInput input,
        BlogDbContext context,
        IValidator<CreateCommentInput> validator,
        ITopicEventSender eventSender,
        ClaimsPrincipal claimsPrincipal)
    {
        // Validate input
        var validationResult = await validator.ValidateAsync(input);
        if (!validationResult.IsValid)
        {
            return new CreateCommentPayload
            {
                Comment = null,
                Errors = validationResult.Errors.Select(e => new UserError(e.ErrorMessage, "VALIDATION_ERROR")).ToList()
            };
        }

        // Get current user
        var userIdClaim = claimsPrincipal.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var authorId))
        {
            return new CreateCommentPayload
            {
                Comment = null,
                Errors = new List<UserError> { new UserError("Authentication required", "AUTHENTICATION_REQUIRED") }
            };
        }

        // Verify post exists
        var post = await context.Posts.FindAsync(input.PostId);
        if (post == null)
        {
            return new CreateCommentPayload
            {
                Comment = null,
                Errors = new List<UserError> { new UserError("Post not found", "POST_NOT_FOUND") }
            };
        }

        // Create comment
        var comment = new Comment
        {
            Content = input.Content,
            PostId = input.PostId,
            AuthorId = authorId,
            ParentCommentId = input.ParentCommentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        // Load the comment with related data
        await context.Entry(comment)
            .Reference(c => c.Author)
            .LoadAsync();

        // Send subscription event
        await eventSender.SendAsync(nameof(Subscription.OnCommentCreated), comment);

        return new CreateCommentPayload
        {
            Comment = comment,
            Errors = new List<UserError>()
        };
    }

    private static string HashPassword(string password)
    {
        // In production, use BCrypt or similar
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"hashed_{password}"));
    }
}
