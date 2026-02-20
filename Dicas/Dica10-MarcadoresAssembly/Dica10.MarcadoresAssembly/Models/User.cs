namespace Dica10.MarcadoresAssembly.Models;

/// <summary>
/// Modelo de usuário para demonstração
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO para criação de usuário
/// </summary>
public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// DTO para exibição de usuário
/// </summary>
public class UserDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FormattedCreatedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
