using Dica80.CleanArchitecture.Domain.Common;
using Dica80.CleanArchitecture.Domain.ValueObjects;
using Dica80.CleanArchitecture.Domain.Events;

namespace Dica80.CleanArchitecture.Domain.Entities;

/// <summary>
/// User entity representing a system user
/// </summary>
public class User : BaseEntity, ISoftDeletable
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }
    
    // Soft delete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    private readonly List<Project> _projects = new();
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    private readonly List<TaskItem> _assignedTasks = new();
    public IReadOnlyCollection<TaskItem> AssignedTasks => _assignedTasks.AsReadOnly();

    // Parameterless constructor for EF
    private User() { }

    private User(string firstName, string lastName, Email email, string passwordHash)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserCreatedEvent(this));
    }

    public static User Create(string firstName, string lastName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        var emailVo = Email.Create(email);
        return new User(firstName, lastName, emailVo, passwordHash);
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedEvent(this));
    }

    public void UpdateEmail(string newEmail)
    {
        var email = Email.Create(newEmail);
        if (Email.Value != email.Value)
        {
            Email = email;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new UserEmailUpdatedEvent(this));
        }
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserDeactivatedEvent(this));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserActivatedEvent(this));
    }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        IsActive = false;
        AddDomainEvent(new UserDeletedEvent(this));
    }
}
