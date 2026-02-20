using Dica80.CleanArchitecture.Domain.Enums;
using Dica80.CleanArchitecture.Domain.ValueObjects;
using TaskStatus = Dica80.CleanArchitecture.Domain.Enums.TaskStatus;

namespace Dica80.CleanArchitecture.Application.DTOs;

/// <summary>
/// DTO for Task data transfer
/// </summary>
public record TaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskStatus Status { get; init; }
    public required Priority Priority { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public Guid? AssignedToId { get; init; }
    public string? AssignedToName { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public int CommentCount { get; init; }
}

/// <summary>
/// DTO for Task creation
/// </summary>
public record CreateTaskDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Priority Priority { get; init; } = Priority.Medium;
    public Guid ProjectId { get; init; }
    public Guid? AssignedToId { get; init; }
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// DTO for Task update
/// </summary>
public record UpdateTaskDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskStatus Status { get; init; }
    public required Priority Priority { get; init; }
    public Guid? AssignedToId { get; init; }
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// DTO for Comment data transfer
/// </summary>
public record CommentDto
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid TaskId { get; init; }
    public string TaskTitle { get; init; } = string.Empty;
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// DTO for Comment creation
/// </summary>
public record CreateCommentDto
{
    public string Content { get; init; } = string.Empty;
    public Guid TaskId { get; init; }
    public Guid AuthorId { get; init; }
}

/// <summary>
/// DTO for Comment update
/// </summary>
public record UpdateCommentDto
{
    public string Content { get; init; } = string.Empty;
}


