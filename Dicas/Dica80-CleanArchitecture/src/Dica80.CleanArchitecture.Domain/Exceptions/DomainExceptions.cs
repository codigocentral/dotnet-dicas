namespace Dica80.CleanArchitecture.Domain.Exceptions;

/// <summary>
/// Base domain exception
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }
    
    public BusinessRuleViolationException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }
    
    public EntityNotFoundException(string entityType, object entityId) 
        : base($"{entityType} with ID {entityId} was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when trying to create a duplicate entity
/// </summary>
public class DuplicateEntityException : DomainException
{
    public string EntityType { get; }
    public string PropertyName { get; }
    public object PropertyValue { get; }
    
    public DuplicateEntityException(string entityType, string propertyName, object propertyValue) 
        : base($"{entityType} with {propertyName} '{propertyValue}' already exists")
    {
        EntityType = entityType;
        PropertyName = propertyName;
        PropertyValue = propertyValue;
    }
}

/// <summary>
/// Exception thrown when an operation is not allowed in the current state
/// </summary>
public class InvalidOperationDomainException : DomainException
{
    public string Operation { get; }
    public string CurrentState { get; }
    
    public InvalidOperationDomainException(string operation, string currentState, string message) 
        : base(message)
    {
        Operation = operation;
        CurrentState = currentState;
    }
}
