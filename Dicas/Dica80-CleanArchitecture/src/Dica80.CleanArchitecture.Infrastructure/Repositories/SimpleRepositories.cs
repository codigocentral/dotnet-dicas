using Microsoft.EntityFrameworkCore;
using Dica80.CleanArchitecture.Domain.Common;
using Dica80.CleanArchitecture.Domain.Entities;
using Dica80.CleanArchitecture.Domain.Enums;
using Dica80.CleanArchitecture.Domain.Repositories;
using Dica80.CleanArchitecture.Infrastructure.Data;
using TaskStatus = Dica80.CleanArchitecture.Domain.Enums.TaskStatus;

namespace Dica80.CleanArchitecture.Infrastructure.Repositories;

/// <summary>
/// Simplified base repository for demonstration purposes
/// </summary>
public abstract class SimpleBaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected SimpleBaseRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            DbSet.Update(entity);
        }
        else
        {
            DbSet.Remove(entity);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Simplified User Repository
/// </summary>
public class SimpleUserRepository : SimpleBaseRepository<User>, IUserRepository
{
    public SimpleUserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(u => u.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken); // Simplified implementation
    }

    public async Task<(IEnumerable<User> users, int totalCount)> GetUsersAsync(int pageNumber, int pageSize, string? searchTerm = null, UserRole? role = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (users, totalCount);
    }

    public async Task<object> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return new { ProjectsOwned = 0, TasksAssigned = 0 }; // Simplified implementation
    }
}

/// <summary>
/// Simplified Project Repository
/// </summary>
public class SimpleProjectRepository : SimpleBaseRepository<Project>, IProjectRepository
{
    public SimpleProjectRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(p => p.OwnerId == ownerId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(p => p.Status == ProjectStatus.InProgress).ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Project?> GetWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(p => p.OwnerId == userId).ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Simplified Task Repository
/// </summary>
public class SimpleTaskRepository : SimpleBaseRepository<TaskItem>, ITaskRepository
{
    public SimpleTaskRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(t => t.ProjectId == projectId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetByAssigneeIdAsync(Guid assigneeId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(t => t.AssigneeId == assigneeId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet.Where(t => t.DueDate.HasValue && t.DueDate < now && t.Status != TaskStatus.Done).ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetWithCommentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetTasksDueSoonAsync(int days, CancellationToken cancellationToken = default)
    {
        var dueDate = DateTime.UtcNow.AddDays(days);
        return await DbSet.Where(t => t.DueDate.HasValue && t.DueDate <= dueDate && t.Status != TaskStatus.Done).ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Simplified Comment Repository
/// </summary>
public class SimpleCommentRepository : SimpleBaseRepository<Comment>, ICommentRepository
{
    public SimpleCommentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Comment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(c => c.TaskId == taskId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(c => c.AuthorId == authorId).ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Simplified Unit of Work
/// </summary>
public class SimpleUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IUserRepository? _users;
    private IProjectRepository? _projects;
    private ITaskRepository? _tasks;
    private ICommentRepository? _comments;

    public SimpleUnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new SimpleUserRepository(_context);
    public IProjectRepository Projects => _projects ??= new SimpleProjectRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new SimpleTaskRepository(_context);
    public ICommentRepository Comments => _comments ??= new SimpleCommentRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
            await _context.Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
            await _context.Database.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
