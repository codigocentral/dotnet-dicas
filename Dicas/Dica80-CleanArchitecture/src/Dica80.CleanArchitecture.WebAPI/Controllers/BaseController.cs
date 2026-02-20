using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dica80.CleanArchitecture.Application.Common;

namespace Dica80.CleanArchitecture.WebAPI.Controllers;

/// <summary>
/// Base controller with common functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected BaseController(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// Returns action result based on Result pattern
    /// </summary>
    /// <typeparam name="T">Result data type</typeparam>
    /// <param name="result">Result to convert</param>
    /// <returns>Action result</returns>
    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        if (result.ValidationErrors.Count > 0)
        {
            return BadRequest(new
            {
                Error = result.Error,
                ValidationErrors = result.ValidationErrors
            });
        }

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Returns action result based on Result pattern (no data)
    /// </summary>
    /// <param name="result">Result to convert</param>
    /// <returns>Action result</returns>
    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        if (result.ValidationErrors.Count > 0)
        {
            return BadRequest(new
            {
                Error = result.Error,
                ValidationErrors = result.ValidationErrors
            });
        }

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Returns paginated result with metadata
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="result">Paginated result</param>
    /// <returns>Action result with pagination metadata</returns>
    protected ActionResult<object> HandlePagedResult<T>(Result<PagedResult<T>> result)
    {
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var response = new
        {
            Data = result.Data!.Items,
            Pagination = new
            {
                result.Data.PageNumber,
                result.Data.PageSize,
                result.Data.TotalCount,
                TotalPages = result.Data.TotalPages,
                HasPrevious = result.Data.HasPrevious,
                HasNext = result.Data.HasNext
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Returns created result with location header
    /// </summary>
    /// <typeparam name="T">Result data type</typeparam>
    /// <param name="result">Result to convert</param>
    /// <param name="actionName">Action name for location</param>
    /// <param name="routeValues">Route values for location</param>
    /// <returns>Created action result</returns>
    protected ActionResult<T> HandleCreatedResult<T>(Result<T> result, string actionName, object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            return CreatedAtAction(actionName, routeValues, result.Data);
        }

        return HandleResult(result);
    }
}
