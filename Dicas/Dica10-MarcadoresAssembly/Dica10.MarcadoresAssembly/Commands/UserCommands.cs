using Dica10.MarcadoresAssembly.Models;

namespace Dica10.MarcadoresAssembly.Commands;

/// <summary>
/// Command MediatR para criação de usuário
/// </summary>
public record CreateUserCommand(string Name, string Email) : IRequest<int>;

/// <summary>
/// Handler para o comando de criação de usuário
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IMapper _mapper;
    private static readonly List<User> _users = new();
    private static int _nextId = 1;

    public CreateUserCommandHandler(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var createDto = new CreateUserDto 
        { 
            Name = request.Name, 
            Email = request.Email 
        };

        var user = _mapper.Map<User>(createDto);
        user.Id = _nextId++;
        
        _users.Add(user);
        
        return Task.FromResult(user.Id);
    }
}

/// <summary>
/// Query MediatR para obtenção de usuários
/// </summary>
public record GetUsersQuery() : IRequest<List<UserDisplayDto>>;

/// <summary>
/// Handler para a query de obtenção de usuários
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDisplayDto>>
{
    private readonly IMapper _mapper;
    private static readonly List<User> _users = new();

    public GetUsersQueryHandler(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Task<List<UserDisplayDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Acessar a mesma lista estática do CreateUserCommandHandler
        var users = GetAllUsers();
        var userDtos = _mapper.Map<List<UserDisplayDto>>(users);
        return Task.FromResult(userDtos);
    }

    private static List<User> GetAllUsers()
    {
        // Em uma aplicação real, isso viria de um repositório
        return new List<User>
        {
            new() { Id = 1, Name = "João Silva", Email = "joao@email.com", CreatedAt = DateTime.UtcNow.AddDays(-5), IsActive = true },
            new() { Id = 2, Name = "Maria Santos", Email = "maria@email.com", CreatedAt = DateTime.UtcNow.AddDays(-3), IsActive = true },
            new() { Id = 3, Name = "Pedro Costa", Email = "pedro@email.com", CreatedAt = DateTime.UtcNow.AddDays(-1), IsActive = false }
        };
    }
}
