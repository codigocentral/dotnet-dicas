using Dica80.CleanArchitecture.Domain.Common;
using Dica80.CleanArchitecture.Domain.Events;

namespace Dica80.CleanArchitecture.Domain.Entities;

/// <summary>
/// Comment entity for task comments
/// </summary>
public class Comment : BaseEntity, ISoftDeletable
{
    public string Content { get; private set; } = string.Empty;
    public Guid TaskId { get; private set; }
    public Guid AuthorId { get; private set; }
    
    // Soft delete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public TaskItem Task { get; private set; } = null!;
    public User Author { get; private set; } = null!;

    // Parameterless constructor for EF
    private Comment() { }

    private Comment(string content, Guid taskId, Guid authorId)
    {
        Content = content;
        TaskId = taskId;
        AuthorId = authorId;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CommentCreatedEvent(this));
    }

    public static Comment Create(string content, Guid taskId, Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content is required", nameof(content));

        if (taskId == Guid.Empty)
            throw new ArgumentException("Valid task ID is required", nameof(taskId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("Valid author ID is required", nameof(authorId));

        return new Comment(content, taskId, authorId);
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content is required", nameof(content));

        Content = content;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CommentUpdatedEvent(this));
    }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        AddDomainEvent(new CommentDeletedEvent(this));
    }
}
