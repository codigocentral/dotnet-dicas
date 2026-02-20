using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dica80.CleanArchitecture.Application.DTOs;
using Dica80.CleanArchitecture.Application.Users.Commands;
using Dica80.CleanArchitecture.Application.Users.Queries;
using Dica80.CleanArchitecture.Application.Common;

namespace Dica80.CleanArchitecture.WebAPI.Controllers;

/// <summary>
/// Controller for User management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : BaseController
{
    public UsersController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all users with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="searchTerm">Search term for name/email</param>
    /// <param name="role">Filter by user role</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Domain.Enums.UserRole? role = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetUsersQuery
        {
            Pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize },
            SearchTerm = searchTerm,
            Role = role,
            IsActive = isActive
        };

        var result = await Mediator.Send(query);
        return HandlePagedResult(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var query = new GetUserByIdQuery { Id = id };
        var result = await Mediator.Send(query);
        
        if (!result.IsSuccess && result.Error == "User not found")
        {
            return NotFound(new { Error = result.Error });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User details</returns>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
    {
        var query = new GetUserByEmailQuery { Email = email };
        var result = await Mediator.Send(query);
        
        if (!result.IsSuccess && result.Error == "User not found")
        {
            return NotFound(new { Error = result.Error });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Get user statistics
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User statistics</returns>
    [HttpGet("{id:guid}/stats")]
    [ProducesResponseType(typeof(UserStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserStatsDto>> GetUserStats(Guid id)
    {
        var query = new GetUserStatsQuery { UserId = id };
        var result = await Mediator.Send(query);
        
        if (!result.IsSuccess && result.Error == "User not found")
        {
            return NotFound(new { Error = result.Error });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto request)
    {
        var command = new CreateUserCommand
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role
        };

        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetUser), new { id = result.Data?.Id });
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update data</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto request)
    {
        var command = new UpdateUserCommand
        {
            Id = id,
            Name = request.Name,
            Role = request.Role,
            IsActive = request.IsActive
        };

        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess && result.Error == "User not found")
        {
            return NotFound(new { Error = result.Error });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand { Id = id };
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess && result.Error == "User not found")
        {
            return NotFound(new { Error = result.Error });
        }

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleResult(result);
    }
}
