namespace Dica44.MediatR.Models;

/// <summary>
/// Modelo de usuário para demonstração
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public User() { }

    public User(string name, string email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string email)
    {
        Name = name;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Resultado de operações
/// </summary>
public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProcessedCount { get; set; }
    public TimeSpan Duration { get; set; }

    public static OperationResult SuccessResult(string message, int count = 0, TimeSpan duration = default)
    {
        return new OperationResult
        {
            Success = true,
            Message = message,
            ProcessedCount = count,
            Duration = duration
        };
    }

    public static OperationResult FailureResult(string message)
    {
        return new OperationResult
        {
            Success = false,
            Message = message
        };
    }
}

/// <summary>
/// Repositório simulado de usuários
/// </summary>
public class UserRepository
{
    private static readonly List<User> _users = new();

    public User Add(User user)
    {
        _users.Add(user);
        return user;
    }

    public User? GetById(Guid id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public IEnumerable<User> GetAll()
    {
        return _users.AsReadOnly();
    }

    public IEnumerable<User> Search(string searchTerm)
    {
        return _users.Where(u => 
            u.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public User Update(User user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            var index = _users.IndexOf(existingUser);
            _users[index] = user;
        }
        return user;
    }

    public bool Delete(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
            return true;
        }
        return false;
    }

    public int Count => _users.Count;
}
