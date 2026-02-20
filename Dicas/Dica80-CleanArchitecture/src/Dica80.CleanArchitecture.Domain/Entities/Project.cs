using Dica80.CleanArchitecture.Domain.Common;
using Dica80.CleanArchitecture.Domain.ValueObjects;
using Dica80.CleanArchitecture.Domain.Events;
using Dica80.CleanArchitecture.Domain.Enums;

namespace Dica80.CleanArchitecture.Domain.Entities;

/// <summary>
/// Project entity representing a work project
/// </summary>
public class Project : BaseEntity, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProjectStatus Status { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Money? Budget { get; private set; }
    public Guid OwnerId { get; private set; }
    
    // Soft delete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public User Owner { get; private set; } = null!;
    
    private readonly List<TaskItem> _tasks = new();
    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    private readonly List<User> _members = new();
    public IReadOnlyCollection<User> Members => _members.AsReadOnly();

    // Parameterless constructor for EF
    private Project() { }

    private Project(string name, string description, Guid ownerId, Money? budget = null)
    {
        Name = name;
        Description = description;
        OwnerId = ownerId;
        Budget = budget;
        Status = ProjectStatus.Planning;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ProjectCreatedEvent(this));
    }

    public static Project Create(string name, string description, Guid ownerId, Money? budget = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Project description is required", nameof(description));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("Valid owner ID is required", nameof(ownerId));

        return new Project(name, description, ownerId, budget);
    }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Project description is required", nameof(description));

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectUpdatedEvent(this));
    }

    public void SetBudget(Money budget)
    {
        Budget = budget ?? throw new ArgumentNullException(nameof(budget));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start(DateTime? startDate = null)
    {
        if (Status != ProjectStatus.Planning)
            throw new InvalidOperationException("Only projects in planning status can be started");

        StartDate = startDate ?? DateTime.UtcNow;
        Status = ProjectStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectStartedEvent(this));
    }

    public void Complete(DateTime? endDate = null)
    {
        if (Status != ProjectStatus.InProgress)
            throw new InvalidOperationException("Only projects in progress can be completed");

        EndDate = endDate ?? DateTime.UtcNow;
        Status = ProjectStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectCompletedEvent(this));
    }

    public void Cancel()
    {
        if (Status == ProjectStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed project");

        Status = ProjectStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectCancelledEvent(this));
    }

    public TaskItem AddTask(string title, string description, Priority priority, Guid? assigneeId = null)
    {
        var task = TaskItem.Create(title, description, priority, Id, assigneeId);
        _tasks.Add(task);
        
        AddDomainEvent(new TaskAddedToProjectEvent(this, task));
        return task;
    }

    public void AddMember(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (_members.Any(m => m.Id == user.Id))
            throw new InvalidOperationException("User is already a member of this project");

        _members.Add(user);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MemberAddedToProjectEvent(this, user));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.Id == userId);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this project");

        if (userId == OwnerId)
            throw new InvalidOperationException("Cannot remove the project owner");

        _members.Remove(member);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MemberRemovedFromProjectEvent(this, member));
    }

    public bool IsCompleted => Status == ProjectStatus.Completed;
    public bool IsActive => Status == ProjectStatus.InProgress;
    public int TaskCount => _tasks.Count;
    public int CompletedTaskCount => _tasks.Count(t => t.IsCompleted);
    public decimal CompletionPercentage => TaskCount == 0 ? 0 : (decimal)CompletedTaskCount / TaskCount * 100;

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        Status = ProjectStatus.Cancelled;
        AddDomainEvent(new ProjectDeletedEvent(this));
    }
}
