using Dica80.CleanArchitecture.Domain.Enums;

namespace Dica80.CleanArchitecture.Application.DTOs;

/// <summary>
/// DTO for User data transfer
/// </summary>
public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

/// <summary>
/// DTO for User creation
/// </summary>
public record CreateUserDto
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.User;
}

/// <summary>
/// DTO for User update
/// </summary>
public record UpdateUserDto
{
    public string Name { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// DTO for Project data transfer
/// </summary>
public record ProjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectStatus Status { get; init; }
    public Guid OwnerId { get; init; }
    public string OwnerName { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? Budget { get; init; }
    public string? BudgetCurrency { get; init; }
    public DateTime CreatedAt { get; init; }
    public int TaskCount { get; init; }
    public int CompletedTaskCount { get; init; }
}

/// <summary>
/// DTO for Project creation
/// </summary>
public record CreateProjectDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid OwnerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
}

/// <summary>
/// DTO for Project update
/// </summary>
public record UpdateProjectDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectStatus Status { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
}
