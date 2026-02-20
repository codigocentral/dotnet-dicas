namespace Dica80.CleanArchitecture.Domain.Enums;

/// <summary>
/// Project status enumeration
/// </summary>
public enum ProjectStatus
{
    Planning = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    OnHold = 5
}

/// <summary>
/// Task status enumeration
/// </summary>
public enum TaskStatus
{
    ToDo = 1,
    InProgress = 2,
    Done = 3,
    Cancelled = 4
}

/// <summary>
/// User role enumeration
/// </summary>
public enum UserRole
{
    User = 1,
    ProjectManager = 2,
    Admin = 3
}
