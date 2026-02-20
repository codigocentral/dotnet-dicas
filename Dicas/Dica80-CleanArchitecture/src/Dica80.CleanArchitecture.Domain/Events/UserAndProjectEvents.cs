using Dica80.CleanArchitecture.Domain.Common;
using Dica80.CleanArchitecture.Domain.Entities;

namespace Dica80.CleanArchitecture.Domain.Events;

// User Events
public class UserCreatedEvent : BaseDomainEvent
{
    public User User { get; }
    
    public UserCreatedEvent(User user)
    {
        User = user;
    }
}

public class UserProfileUpdatedEvent : BaseDomainEvent
{
    public User User { get; }
    
    public UserProfileUpdatedEvent(User user)
    {
        User = user;
    }
}

public class UserEmailUpdatedEvent : BaseDomainEvent
{
    public User User { get; }
    
    public UserEmailUpdatedEvent(User user)
    {
        User = user;
    }
}

public class UserActivatedEvent : BaseDomainEvent
{
    public User User { get; }
    
    public UserActivatedEvent(User user)
    {
        User = user;
    }
}

public class UserDeactivatedEvent : BaseDomainEvent
{
    public User User { get; }
    
    public UserDeactivatedEvent(User user)
    {
        User = user;
    }
}

public class UserDeletedEvent : BaseDomainEvent
{
    public User User { get; }
    
    public UserDeletedEvent(User user)
    {
        User = user;
    }
}

// Project Events
public class ProjectCreatedEvent : BaseDomainEvent
{
    public Project Project { get; }
    
    public ProjectCreatedEvent(Project project)
    {
        Project = project;
    }
}

public class ProjectUpdatedEvent : BaseDomainEvent
{
    public Project Project { get; }
    
    public ProjectUpdatedEvent(Project project)
    {
        Project = project;
    }
}

public class ProjectStartedEvent : BaseDomainEvent
{
    public Project Project { get; }
    
    public ProjectStartedEvent(Project project)
    {
        Project = project;
    }
}

public class ProjectCompletedEvent : BaseDomainEvent
{
    public Project Project { get; }
    
    public ProjectCompletedEvent(Project project)
    {
        Project = project;
    }
}

public class ProjectCancelledEvent : BaseDomainEvent
{
    public Project Project { get; }
    
    public ProjectCancelledEvent(Project project)
    {
        Project = project;
    }
}

public class ProjectDeletedEvent : BaseDomainEvent
{
    public Project Project { get; }
    
    public ProjectDeletedEvent(Project project)
    {
        Project = project;
    }
}

public class MemberAddedToProjectEvent : BaseDomainEvent
{
    public Project Project { get; }
    public User Member { get; }
    
    public MemberAddedToProjectEvent(Project project, User member)
    {
        Project = project;
        Member = member;
    }
}

public class MemberRemovedFromProjectEvent : BaseDomainEvent
{
    public Project Project { get; }
    public User Member { get; }
    
    public MemberRemovedFromProjectEvent(Project project, User member)
    {
        Project = project;
        Member = member;
    }
}
