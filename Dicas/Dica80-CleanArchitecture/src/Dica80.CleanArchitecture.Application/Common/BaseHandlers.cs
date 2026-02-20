using AutoMapper;
using MediatR;

namespace Dica80.CleanArchitecture.Application.Common;

/// <summary>
/// Base class for commands that modify data
/// </summary>
public abstract record BaseCommand : IRequest;

/// <summary>
/// Base class for commands that return data
/// </summary>
/// <typeparam name="TResponse">Type of response</typeparam>
public abstract record BaseCommand<TResponse> : IRequest<TResponse>;

/// <summary>
/// Base class for queries that return data
/// </summary>
/// <typeparam name="TResponse">Type of response</typeparam>
public abstract record BaseQuery<TResponse> : IRequest<TResponse>;

/// <summary>
/// Base class for command handlers
/// </summary>
/// <typeparam name="TCommand">Type of command</typeparam>
public abstract class BaseCommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : BaseCommand
{
    protected readonly IMapper Mapper;

    protected BaseCommandHandler(IMapper mapper)
    {
        Mapper = mapper;
    }

    public abstract Task Handle(TCommand request, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for command handlers with response
/// </summary>
/// <typeparam name="TCommand">Type of command</typeparam>
/// <typeparam name="TResponse">Type of response</typeparam>
public abstract class BaseCommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : BaseCommand<TResponse>
{
    protected readonly IMapper Mapper;

    protected BaseCommandHandler(IMapper mapper)
    {
        Mapper = mapper;
    }

    public abstract Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for query handlers
/// </summary>
/// <typeparam name="TQuery">Type of query</typeparam>
/// <typeparam name="TResponse">Type of response</typeparam>
public abstract class BaseQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : BaseQuery<TResponse>
{
    protected readonly IMapper Mapper;

    protected BaseQueryHandler(IMapper mapper)
    {
        Mapper = mapper;
    }

    public abstract Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken);
}

/// <summary>
/// Result wrapper for operations
/// </summary>
/// <typeparam name="T">Type of result data</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }
    public List<string> ValidationErrors { get; private set; } = new();

    private Result(bool isSuccess, T? data, string? error, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> ValidationFailure(List<string> errors) => new(false, default, "Validation failed", errors);
}

/// <summary>
/// Result wrapper for operations without return data
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }
    public List<string> ValidationErrors { get; private set; } = new();

    private Result(bool isSuccess, string? error, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result ValidationFailure(List<string> errors) => new(false, "Validation failed", errors);
}
