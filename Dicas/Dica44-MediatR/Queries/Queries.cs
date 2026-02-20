using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Dica44.MediatR.Models;

namespace Dica44.MediatR.Queries;

// ================================
// GET USER BY ID QUERY
// ================================

/// <summary>
/// Query para buscar um usuário por ID
/// </summary>
public record GetUserByIdQuery(Guid Id) : IRequest<User?>;

/// <summary>
/// Validator para GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID é obrigatório");
    }
}

/// <summary>
/// Handler para GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User?>
{
    private readonly UserRepository _userRepository;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(UserRepository userRepository, ILogger<GetUserByIdQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando usuário por ID: {UserId}", request.Id);

        // Simular latência de acesso ao banco
        await Task.Delay(25, cancellationToken);

        var user = _userRepository.GetById(request.Id);
        
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado: {UserId}", request.Id);
        }
        else
        {
            _logger.LogInformation("Usuário encontrado: {UserId} - {Name}", user.Id, user.Name);
        }

        return user;
    }
}

// ================================
// GET ALL USERS QUERY
// ================================

/// <summary>
/// Query para listar todos os usuários
/// </summary>
public record GetAllUsersQuery : IRequest<IEnumerable<User>>;

/// <summary>
/// Handler para GetAllUsersQuery
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<User>>
{
    private readonly UserRepository _userRepository;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(UserRepository userRepository, ILogger<GetAllUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listando todos os usuários");

        await Task.Delay(30, cancellationToken);

        var users = _userRepository.GetAll();
        _logger.LogInformation("Encontrados {Count} usuários", users.Count());

        return users;
    }
}

// ================================
// SEARCH USERS QUERY
// ================================

/// <summary>
/// Query para buscar usuários com paginação e filtros
/// </summary>
public record SearchUsersQuery(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 10) : IRequest<SearchUsersResult>;

/// <summary>
/// Resultado da busca de usuários
/// </summary>
public record SearchUsersResult(
    IEnumerable<User> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Validator para SearchUsersQuery
/// </summary>
public class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Página deve ser maior que 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Tamanho da página deve ser maior que 0")
            .LessThanOrEqualTo(100).WithMessage("Tamanho da página deve ser no máximo 100");

        When(x => !string.IsNullOrWhiteSpace(x.SearchTerm), () =>
        {
            RuleFor(x => x.SearchTerm)
                .MinimumLength(2).WithMessage("Termo de busca deve ter pelo menos 2 caracteres");
        });
    }
}

/// <summary>
/// Handler para SearchUsersQuery
/// </summary>
public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, SearchUsersResult>
{
    private readonly UserRepository _userRepository;
    private readonly ILogger<SearchUsersQueryHandler> _logger;

    public SearchUsersQueryHandler(UserRepository userRepository, ILogger<SearchUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<SearchUsersResult> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando usuários - Termo: {SearchTerm}, Página: {Page}, Tamanho: {PageSize}", 
            request.SearchTerm, request.Page, request.PageSize);

        await Task.Delay(50, cancellationToken);

        var allUsers = _userRepository.GetAll();

        // Aplicar filtro de busca
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLowerInvariant();
            allUsers = allUsers.Where(u => 
                u.Name.ToLowerInvariant().Contains(searchTerm) ||
                u.Email.ToLowerInvariant().Contains(searchTerm));
        }

        var totalCount = allUsers.Count();

        // Aplicar paginação
        var users = allUsers
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        _logger.LogInformation("Busca concluída - Encontrados: {TotalCount}, Retornados: {ReturnedCount}", 
            totalCount, users.Count);

        return new SearchUsersResult(
            users,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages);
    }
}

// ================================
// GET USER STATISTICS QUERY
// ================================

/// <summary>
/// Query para obter estatísticas dos usuários
/// </summary>
public record GetUserStatisticsQuery : IRequest<UserStatistics>;

/// <summary>
/// Estatísticas dos usuários
/// </summary>
public record UserStatistics(
    int TotalUsers,
    DateTime? OldestUserCreated,
    DateTime? NewestUserCreated,
    int UsersCreatedToday,
    int UsersCreatedThisWeek,
    int UsersCreatedThisMonth);

/// <summary>
/// Handler para GetUserStatisticsQuery
/// </summary>
public class GetUserStatisticsQueryHandler : IRequestHandler<GetUserStatisticsQuery, UserStatistics>
{
    private readonly UserRepository _userRepository;
    private readonly ILogger<GetUserStatisticsQueryHandler> _logger;

    public GetUserStatisticsQueryHandler(UserRepository userRepository, ILogger<GetUserStatisticsQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserStatistics> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculando estatísticas dos usuários");

        await Task.Delay(75, cancellationToken);

        var users = _userRepository.GetAll().ToList();
        var totalUsers = users.Count;

        DateTime? oldestUserCreated = null;
        DateTime? newestUserCreated = null;

        if (users.Any())
        {
            oldestUserCreated = users.Min(u => u.CreatedAt);
            newestUserCreated = users.Max(u => u.CreatedAt);
        }

        var now = DateTime.UtcNow;
        var today = now.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var usersCreatedToday = users.Count(u => u.CreatedAt.Date == today);
        var usersCreatedThisWeek = users.Count(u => u.CreatedAt.Date >= startOfWeek);
        var usersCreatedThisMonth = users.Count(u => u.CreatedAt.Date >= startOfMonth);

        var statistics = new UserStatistics(
            totalUsers,
            oldestUserCreated,
            newestUserCreated,
            usersCreatedToday,
            usersCreatedThisWeek,
            usersCreatedThisMonth);

        _logger.LogInformation("Estatísticas calculadas - Total: {TotalUsers}, Hoje: {Today}, Semana: {Week}, Mês: {Month}", 
            totalUsers, usersCreatedToday, usersCreatedThisWeek, usersCreatedThisMonth);

        return statistics;
    }
}
