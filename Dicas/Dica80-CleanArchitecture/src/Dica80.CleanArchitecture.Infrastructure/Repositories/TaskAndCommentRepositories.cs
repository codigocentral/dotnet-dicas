using Microsoft.EntityFrameworkCore;
using Dica80.CleanArchitecture.Domain.Entities;
using Dica80.CleanArchitecture.Domain.Enums;
using Dica80.CleanArchitecture.Domain.Repositories;
using Dica80.CleanArchitecture.Infrastructure.Data;
using TaskStatus = Dica80.CleanArchitecture.Domain.Enums.TaskStatus;

namespace Dica80.CleanArchitecture.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TaskItem entity
/// </summary>
public class TaskRepository : BaseRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Assignee)
            .Where(t => t.ProjectId == projectId && !t.IsDeleted)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetByAssigneeIdAsync(Guid assigneeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Project)
            .Where(t => t.AssigneeId == assigneeId && !t.IsDeleted)
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        
        return await DbSet
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Where(t => t.DueDate.HasValue && 
                       t.DueDate.Value < today && 
                       t.Status != TaskStatus.Done &&
                       !t.IsDeleted)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetWithCommentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetTasksDueSoonAsync(int days, CancellationToken cancellationToken = default)
    {
        var dueDate = DateTime.UtcNow.AddDays(days);
        
        return await DbSet
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Where(t => t.DueDate.HasValue && 
                       t.DueDate.Value <= dueDate && 
                       t.Status != TaskStatus.Done &&
                       !t.IsDeleted)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    protected override IQueryable<TaskItem> ApplyIncludes(IQueryable<TaskItem> query)
    {
        return query
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Comments);
    }
}

/// <summary>
/// Repository implementation for Comment entity
/// </summary>
public class CommentRepository : BaseRepository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Comment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Author)
            .Where(c => c.TaskId == taskId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Task)
            .Where(c => c.AuthorId == authorId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    protected override IQueryable<Comment> ApplyIncludes(IQueryable<Comment> query)
    {
        return query
            .Include(c => c.Task)
            .Include(c => c.Author);
    }
}
