using Dica80.CleanArchitecture.Domain.Common;
using Dica80.CleanArchitecture.Domain.ValueObjects;
using Dica80.CleanArchitecture.Domain.Events;
using Dica80.CleanArchitecture.Domain.Enums;
using TaskStatus = Dica80.CleanArchitecture.Domain.Enums.TaskStatus;

namespace Dica80.CleanArchitecture.Domain.Entities;

/// <summary>
/// TaskItem entity representing a work task
/// </summary>
public class TaskItem : BaseEntity, ISoftDeletable
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskStatus Status { get; private set; }
    public Priority Priority { get; private set; } = null!;
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid? AssigneeId { get; private set; }
    
    // Soft delete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public Project Project { get; private set; } = null!;
    public User? Assignee { get; private set; }

    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    // Parameterless constructor for EF
    private TaskItem() { }

    private TaskItem(string title, string description, Priority priority, Guid projectId, Guid? assigneeId = null)
    {
        Title = title;
        Description = description;
        Priority = priority;
        ProjectId = projectId;
        AssigneeId = assigneeId;
        Status = TaskStatus.ToDo;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new TaskCreatedEvent(this));
    }

    public static TaskItem Create(string title, string description, Priority priority, Guid projectId, Guid? assigneeId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Task description is required", nameof(description));

        if (priority == null)
            throw new ArgumentNullException(nameof(priority));

        if (projectId == Guid.Empty)
            throw new ArgumentException("Valid project ID is required", nameof(projectId));

        return new TaskItem(title, description, priority, projectId, assigneeId);
    }

    public void UpdateDetails(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Task description is required", nameof(description));

        Title = title;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskUpdatedEvent(this));
    }

    public void SetPriority(Priority priority)
    {
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskPriorityChangedEvent(this));
    }

    public void SetDueDate(DateTime dueDate)
    {
        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Due date must be in the future", nameof(dueDate));

        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskDueDateSetEvent(this));
    }

    public void Assign(Guid assigneeId)
    {
        if (assigneeId == Guid.Empty)
            throw new ArgumentException("Valid assignee ID is required", nameof(assigneeId));

        var previousAssigneeId = AssigneeId;
        AssigneeId = assigneeId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskAssignedEvent(this, previousAssigneeId, assigneeId));
    }

    public void Unassign()
    {
        var previousAssigneeId = AssigneeId;
        AssigneeId = null;
        UpdatedAt = DateTime.UtcNow;

        if (previousAssigneeId.HasValue)
        {
            AddDomainEvent(new TaskUnassignedEvent(this, previousAssigneeId.Value));
        }
    }

    public void Start()
    {
        if (Status != TaskStatus.ToDo)
            throw new InvalidOperationException("Only tasks in ToDo status can be started");

        Status = TaskStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskStartedEvent(this));
    }

    public void Complete()
    {
        if (Status == TaskStatus.Done)
            throw new InvalidOperationException("Task is already completed");

        Status = TaskStatus.Done;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskCompletedEvent(this));
    }

    public void Reopen()
    {
        if (Status != TaskStatus.Done)
            throw new InvalidOperationException("Only completed tasks can be reopened");

        Status = TaskStatus.InProgress;
        CompletedAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskReopenedEvent(this));
    }

    public Comment AddComment(string content, Guid authorId)
    {
        var comment = Comment.Create(content, Id, authorId);
        _comments.Add(comment);
        
        AddDomainEvent(new CommentAddedToTaskEvent(this, comment));
        return comment;
    }

    public bool IsCompleted => Status == TaskStatus.Done;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsCompleted;
    public bool IsAssigned => AssigneeId.HasValue;
    public int DaysUntilDue => DueDate.HasValue ? (int)(DueDate.Value - DateTime.UtcNow).TotalDays : 0;

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        AddDomainEvent(new TaskDeletedEvent(this));
    }
}
