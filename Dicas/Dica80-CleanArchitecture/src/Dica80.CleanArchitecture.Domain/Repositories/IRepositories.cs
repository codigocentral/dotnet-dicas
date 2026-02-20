using Dica80.CleanArchitecture.Domain.Entities;

namespace Dica80.CleanArchitecture.Domain.Repositories;

/// <summary>
/// Base repository interface with common operations
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> users, int totalCount)> GetUsersAsync(int pageNumber, int pageSize, string? searchTerm = null, Domain.Enums.UserRole? role = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<object> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Project repository interface
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);
    Task<Project?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Task repository interface
/// </summary>
public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetByAssigneeIdAsync(Guid assigneeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(CancellationToken cancellationToken = default);
    Task<TaskItem?> GetWithCommentsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetTasksDueSoonAsync(int days, CancellationToken cancellationToken = default);
}

/// <summary>
/// Comment repository interface
/// </summary>
public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work pattern interface
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    ICommentRepository Comments { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
