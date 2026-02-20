using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Subscriptions;
using Dica79.GraphQLHotChocolate.Models;

namespace Dica79.GraphQLHotChocolate.Types;

[SubscriptionType]
public class Subscription
{
    /// <summary>
    /// Subscribe to post creation events
    /// </summary>
    [Subscribe]
    [Topic]
    public PostSubscriptionPayload OnPostCreated([EventMessage] PostSubscriptionPayload payload)
        => payload;

    /// <summary>
    /// Subscribe to post update events
    /// </summary>
    [Subscribe]
    [Topic]
    public PostSubscriptionPayload OnPostUpdated([EventMessage] PostSubscriptionPayload payload)
        => payload;

    /// <summary>
    /// Subscribe to post deletion events
    /// </summary>
    [Subscribe]
    [Topic]
    public PostSubscriptionPayload OnPostDeleted([EventMessage] PostSubscriptionPayload payload)
        => payload;

    /// <summary>
    /// Subscribe to all post events
    /// </summary>
    [Subscribe]
    public PostSubscriptionPayload OnPostEvent([EventMessage] PostSubscriptionPayload payload)
        => payload;

    /// <summary>
    /// Subscribe to comments for a specific post
    /// </summary>
    [Subscribe]
    [Topic("comments_{postId}")]
    public CommentSubscriptionPayload OnCommentAdded(
        int postId,
        [EventMessage] CommentSubscriptionPayload payload)
        => payload;

    /// <summary>
    /// Subscribe to comments by a specific user
    /// </summary>
    [Subscribe]
    public CommentSubscriptionPayload OnUserComment(
        int userId,
        [EventMessage] CommentSubscriptionPayload payload)
        => payload.Comment.AuthorId == userId ? payload : null!;

    /// <summary>
    /// Subscribe to new user registrations
    /// </summary>
    [Subscribe]
    [Topic]
    public User OnUserCreated([EventMessage] User user)
        => user;

    /// <summary>
    /// Subscribe to user updates
    /// </summary>
    [Subscribe]
    [Topic]
    public User OnUserUpdated([EventMessage] User user)
        => user;

    /// <summary>
    /// Subscribe to user registrations
    /// </summary>
    [Subscribe]
    [Topic]
    public User OnUserRegistered([EventMessage] User user)
        => user;

    /// <summary>
    /// Subscribe to comment creation
    /// </summary>
    [Subscribe]
    [Topic]
    public Comment OnCommentCreated([EventMessage] Comment comment)
        => comment;

    /// <summary>
    /// Subscribe to real-time blog statistics updates
    /// </summary>
    [Subscribe]
    [Topic]
    public BlogStatsUpdate OnBlogStatsUpdated([EventMessage] BlogStatsUpdate update)
        => update;
}

// Additional types for subscriptions
public class BlogStatsUpdate
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int TotalPosts { get; set; }
    public int TotalUsers { get; set; }
    public int TotalComments { get; set; }
    public string UpdateType { get; set; } = string.Empty; // "POST_CREATED", "USER_REGISTERED", etc.
}
