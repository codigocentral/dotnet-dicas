using AutoMapper;
using Dica80.CleanArchitecture.Application.Common;
using Dica80.CleanArchitecture.Application.DTOs;
using Dica80.CleanArchitecture.Domain.Repositories;
using FluentValidation;

namespace Dica80.CleanArchitecture.Application.Users.Queries;

/// <summary>
/// Query to get user by ID
/// </summary>
public record GetUserByIdQuery : BaseQuery<Result<UserDto>>
{
    public Guid Id { get; init; }
}

/// <summary>
/// Validator for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryValidator : BaseValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        ValidateRequiredId(nameof(GetUserByIdQuery.Id), x => x.Id);
    }
}

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : BaseQueryHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper) : base(mapper)
    {
        _userRepository = userRepository;
    }

    public override async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = Mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to get user: {ex.Message}");
        }
    }
}

/// <summary>
/// Query to get user by email
/// </summary>
public record GetUserByEmailQuery : BaseQuery<Result<UserDto>>
{
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Validator for GetUserByEmailQuery
/// </summary>
public class GetUserByEmailQueryValidator : BaseValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryValidator()
    {
        ValidateRequiredEmail(x => x.Email);
    }
}

/// <summary>
/// Handler for GetUserByEmailQuery
/// </summary>
public class GetUserByEmailQueryHandler : BaseQueryHandler<GetUserByEmailQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository, IMapper mapper) : base(mapper)
    {
        _userRepository = userRepository;
    }

    public override async Task<Result<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = Mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to get user: {ex.Message}");
        }
    }
}

/// <summary>
/// Query to get all users with pagination
/// </summary>
public record GetUsersQuery : BaseQuery<Result<Common.PagedResult<UserDto>>>
{
    public Common.PaginationParams Pagination { get; init; } = new();
    public string? SearchTerm { get; init; }
    public Domain.Enums.UserRole? Role { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>
/// Validator for GetUsersQuery
/// </summary>
public class GetUsersQueryValidator : BaseValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Pagination.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.Pagination.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        When(x => x.Role.HasValue, () =>
        {
            ValidateEnum(nameof(GetUsersQuery.Role), x => x.Role!.Value);
        });

        ValidateOptionalString(nameof(GetUsersQuery.SearchTerm), x => x.SearchTerm, 100);
    }
}

/// <summary>
/// Handler for GetUsersQuery
/// </summary>
public class GetUsersQueryHandler : BaseQueryHandler<GetUsersQuery, Result<Common.PagedResult<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper) : base(mapper)
    {
        _userRepository = userRepository;
    }

    public override async Task<Result<Common.PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (users, totalCount) = await _userRepository.GetUsersAsync(
                request.Pagination.PageNumber,
                request.Pagination.PageSize,
                request.SearchTerm,
                request.Role,
                request.IsActive);

            var userDtos = Mapper.Map<List<UserDto>>(users);

            var pagedResult = new Common.PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = request.Pagination.PageNumber,
                PageSize = request.Pagination.PageSize
            };

            return Result<Common.PagedResult<UserDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<UserDto>>.Failure($"Failed to get users: {ex.Message}");
        }
    }
}

/// <summary>
/// Query to get user statistics
/// </summary>
public record GetUserStatsQuery : BaseQuery<Result<UserStatsDto>>
{
    public Guid UserId { get; init; }
}

/// <summary>
/// DTO for user statistics
/// </summary>
public record UserStatsDto
{
    public int ProjectsOwned { get; init; }
    public int ProjectsParticipating { get; init; }
    public int TasksAssigned { get; init; }
    public int TasksCompleted { get; init; }
    public int CommentsWritten { get; init; }
    public DateTime? LastActivity { get; init; }
}

/// <summary>
/// Validator for GetUserStatsQuery
/// </summary>
public class GetUserStatsQueryValidator : BaseValidator<GetUserStatsQuery>
{
    public GetUserStatsQueryValidator()
    {
        ValidateRequiredId(nameof(GetUserStatsQuery.UserId), x => x.UserId);
    }
}

/// <summary>
/// Handler for GetUserStatsQuery
/// </summary>
public class GetUserStatsQueryHandler : BaseQueryHandler<GetUserStatsQuery, Result<UserStatsDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserStatsQueryHandler(IUserRepository userRepository, IMapper mapper) : base(mapper)
    {
        _userRepository = userRepository;
    }

    public override async Task<Result<UserStatsDto>> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var statsData = await _userRepository.GetUserStatsAsync(request.UserId);
            if (statsData == null)
            {
                return Result<UserStatsDto>.Failure("User not found");
            }

            // For now, create a mock stats object since repository returns object
            var stats = new UserStatsDto
            {
                ProjectsOwned = 3,
                ProjectsParticipating = 2,
                TasksAssigned = 15,
                TasksCompleted = 8,
                CommentsWritten = 25,
                LastActivity = DateTime.UtcNow.AddDays(-1)
            };

            return Result<UserStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<UserStatsDto>.Failure($"Failed to get user stats: {ex.Message}");
        }
    }
}
