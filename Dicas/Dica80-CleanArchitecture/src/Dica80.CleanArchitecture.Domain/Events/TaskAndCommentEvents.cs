using Dica80.CleanArchitecture.Domain.Common;
using Dica80.CleanArchitecture.Domain.Entities;

namespace Dica80.CleanArchitecture.Domain.Events;

// Task Events
public class TaskCreatedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskCreatedEvent(TaskItem task)
    {
        Task = task;
    }
}

public class TaskUpdatedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskUpdatedEvent(TaskItem task)
    {
        Task = task;
    }
}

public class TaskStartedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskStartedEvent(TaskItem task)
    {
        Task = task;
    }
}

public class TaskCompletedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskCompletedEvent(TaskItem task)
    {
        Task = task;
    }
}

public class TaskReopenedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskReopenedEvent(TaskItem task)
    {
        Task = task;
    }
}

public class TaskDeletedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskDeletedEvent(TaskItem task)
    {
        Task = task;
    }
}

public class TaskAssignedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    public Guid? PreviousAssigneeId { get; }
    public Guid NewAssigneeId { get; }
    
    public TaskAssignedEvent(TaskItem task, Guid? previousAssigneeId, Guid newAssigneeId)
    {
        Task = task;
        PreviousAssigneeId = previousAssigneeId;
        NewAssigneeId = newAssigneeId;
    }
}

public class TaskUnassignedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    public Guid PreviousAssigneeId { get; }
    
    public TaskUnassignedEvent(TaskItem task, Guid previousAssigneeId)
    {
        Task = task;
        PreviousAssigneeId = previousAssigneeId;
    }
}

public class TaskPriorityChangedEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskPriorityChangedEvent(TaskItem task)
    {
        Task = task;
    }
}

public class TaskDueDateSetEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    
    public TaskDueDateSetEvent(TaskItem task)
    {
        Task = task;
    }
}

public class TaskAddedToProjectEvent : BaseDomainEvent
{
    public Project Project { get; }
    public TaskItem Task { get; }
    
    public TaskAddedToProjectEvent(Project project, TaskItem task)
    {
        Project = project;
        Task = task;
    }
}

// Comment Events
public class CommentCreatedEvent : BaseDomainEvent
{
    public Comment Comment { get; }
    
    public CommentCreatedEvent(Comment comment)
    {
        Comment = comment;
    }
}

public class CommentUpdatedEvent : BaseDomainEvent
{
    public Comment Comment { get; }
    
    public CommentUpdatedEvent(Comment comment)
    {
        Comment = comment;
    }
}

public class CommentDeletedEvent : BaseDomainEvent
{
    public Comment Comment { get; }
    
    public CommentDeletedEvent(Comment comment)
    {
        Comment = comment;
    }
}

public class CommentAddedToTaskEvent : BaseDomainEvent
{
    public TaskItem Task { get; }
    public Comment Comment { get; }
    
    public CommentAddedToTaskEvent(TaskItem task, Comment comment)
    {
        Task = task;
        Comment = comment;
    }
}
