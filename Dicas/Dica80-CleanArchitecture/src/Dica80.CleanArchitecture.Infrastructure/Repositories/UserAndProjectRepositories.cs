using Microsoft.EntityFrameworkCore;
using Dica80.CleanArchitecture.Domain.Entities;
using Dica80.CleanArchitecture.Domain.Enums;
using Dica80.CleanArchitecture.Domain.Repositories;
using Dica80.CleanArchitecture.Infrastructure.Data;
using Dica80.CleanArchitecture.Application.Users.Queries;
using TaskStatus = Dica80.CleanArchitecture.Domain.Enums.TaskStatus;

namespace Dica80.CleanArchitecture.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.Email.Value == email.ToLowerInvariant())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<User>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.AssignedTasks.Any(t => t.ProjectId == projectId && !t.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> users, int totalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLowerInvariant();
            query = query.Where(u => 
                u.FullName.ToLower().Contains(lowerSearchTerm) || 
                u.Email.Value.Contains(lowerSearchTerm));
        }

        if (role.HasValue)
        {
            // Since we don't have Role property in User entity, skip this filter for now
            // query = query.Where(u => u.Role == role.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<object> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var stats = await DbSet
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                ProjectsOwned = u.Projects.Count(p => !p.IsDeleted),
                ProjectsParticipating = u.AssignedTasks
                    .Where(t => !t.IsDeleted && !t.Project.IsDeleted)
                    .Select(t => t.Project)
                    .Distinct()
                    .Count(),
                TasksAssigned = u.AssignedTasks.Count(t => !t.IsDeleted),
                TasksCompleted = u.AssignedTasks.Count(t => !t.IsDeleted && t.Status == TaskStatus.Done),
                CommentsWritten = 0, // Comments relation needs to be checked
                LastActivity = u.AssignedTasks
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.UpdatedAt)
                    .Select(t => t.UpdatedAt)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new object();
    }

    protected override IQueryable<User> ApplyIncludes(IQueryable<User> query)
    {
        return query
            .Include(u => u.Projects)
            .Include(u => u.AssignedTasks);
    }
}

/// <summary>
/// Repository implementation for Project entity
/// </summary>
public class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Tasks)
            .Where(p => p.OwnerId == ownerId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Owner)
            .Include(p => p.Tasks)
            .Where(p => p.Status == ProjectStatus.InProgress && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Tasks)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<Project?> GetWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Tasks)
                .ThenInclude(t => t.Assignee)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Owner)
            .Where(p => (p.OwnerId == userId || p.Tasks.Any(t => t.AssigneeId == userId)) && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Project> Projects, int TotalCount)> GetProjectsAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        ProjectStatus? status = null,
        Guid? ownerId = null)
    {
        var query = DbSet.Include(p => p.Owner).AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLowerInvariant();
            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerSearchTerm) || 
                (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)));
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (ownerId.HasValue)
        {
            query = query.Where(p => p.OwnerId == ownerId.Value);
        }

        return await GetPagedAsync(query, pageNumber, pageSize, p => p.CreatedAt, descending: true);
    }

    public async Task<IEnumerable<Project>> GetProjectsDueSoonAsync(int days = 7)
    {
        var dueDate = DateTime.UtcNow.AddDays(days);
        
        return await DbSet
            .Include(p => p.Owner)
            .Where(p => p.EndDate.HasValue && 
                       p.EndDate.Value <= dueDate && 
                       p.Status == ProjectStatus.InProgress)
            .OrderBy(p => p.EndDate)
            .ToListAsync();
    }

    protected override IQueryable<Project> ApplyIncludes(IQueryable<Project> query)
    {
        return query
            .Include(p => p.Owner)
            .Include(p => p.Tasks);
    }
}
