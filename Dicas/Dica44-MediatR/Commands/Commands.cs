using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Dica44.MediatR.Models;

namespace Dica44.MediatR.Commands;

// ================================
// CREATE USER COMMAND
// ================================

/// <summary>
/// Command para criar um novo usuário
/// </summary>
public record CreateUserCommand(string Name, string Email) : IRequest<User>;

/// <summary>
/// Validator para CreateUserCommand
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter pelo menos 2 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(255).WithMessage("Email deve ter no máximo 255 caracteres");
    }
}

/// <summary>
/// Handler para CreateUserCommand
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
{
    private readonly UserRepository _userRepository;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(UserRepository userRepository, ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando usuário: {Name} - {Email}", request.Name, request.Email);

        // Simular algum processamento assíncrono
        await Task.Delay(100, cancellationToken);

        // Verificar se email já existe
        var existingUsers = _userRepository.GetAll();
        if (existingUsers.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Usuário com email {request.Email} já existe");
        }

        var user = new User(request.Name, request.Email);
        _userRepository.Add(user);

        _logger.LogInformation("Usuário criado com sucesso: {UserId}", user.Id);
        return user;
    }
}

// ================================
// UPDATE USER COMMAND
// ================================

/// <summary>
/// Command para atualizar um usuário existente
/// </summary>
public record UpdateUserCommand(Guid Id, string Name, string Email) : IRequest<User>;

/// <summary>
/// Validator para UpdateUserCommand
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID é obrigatório");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter pelo menos 2 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(255).WithMessage("Email deve ter no máximo 255 caracteres");
    }
}

/// <summary>
/// Handler para UpdateUserCommand
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, User>
{
    private readonly UserRepository _userRepository;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(UserRepository userRepository, ILogger<UpdateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando usuário: {UserId}", request.Id);

        await Task.Delay(50, cancellationToken);

        var user = _userRepository.GetById(request.Id);
        if (user == null)
        {
            throw new InvalidOperationException($"Usuário com ID {request.Id} não encontrado");
        }

        user.Update(request.Name, request.Email);
        _userRepository.Update(user);

        _logger.LogInformation("Usuário atualizado com sucesso: {UserId}", user.Id);
        return user;
    }
}

// ================================
// SLOW OPERATION COMMAND
// ================================

/// <summary>
/// Command que simula uma operação lenta para demonstrar pipeline behaviors
/// </summary>
public record SlowOperationCommand(string Operation, int DelayMs) : IRequest<OperationResult>;

/// <summary>
/// Handler para SlowOperationCommand
/// </summary>
public class SlowOperationCommandHandler : IRequestHandler<SlowOperationCommand, OperationResult>
{
    private readonly ILogger<SlowOperationCommandHandler> _logger;

    public SlowOperationCommandHandler(ILogger<SlowOperationCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<OperationResult> Handle(SlowOperationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando operação lenta: {Operation} ({DelayMs}ms)", 
            request.Operation, request.DelayMs);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Task.Delay(request.DelayMs, cancellationToken);
        stopwatch.Stop();

        _logger.LogInformation("Operação lenta concluída em: {Duration}ms", stopwatch.ElapsedMilliseconds);

        return OperationResult.SuccessResult(
            $"Operação '{request.Operation}' concluída",
            1,
            stopwatch.Elapsed);
    }
}

// ================================
// PROCESS DATA COMMAND
// ================================

/// <summary>
/// Command para processar dados em lote
/// </summary>
public record ProcessDataCommand(string[] Data) : IRequest<OperationResult>;

/// <summary>
/// Validator para ProcessDataCommand
/// </summary>
public class ProcessDataCommandValidator : AbstractValidator<ProcessDataCommand>
{
    public ProcessDataCommandValidator()
    {
        RuleFor(x => x.Data)
            .NotNull().WithMessage("Dados são obrigatórios")
            .Must(data => data.Length > 0).WithMessage("Pelo menos um item deve ser fornecido")
            .Must(data => data.Length <= 1000).WithMessage("Máximo de 1000 itens permitidos");
    }
}

/// <summary>
/// Handler para ProcessDataCommand
/// </summary>
public class ProcessDataCommandHandler : IRequestHandler<ProcessDataCommand, OperationResult>
{
    private readonly ILogger<ProcessDataCommandHandler> _logger;

    public ProcessDataCommandHandler(ILogger<ProcessDataCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<OperationResult> Handle(ProcessDataCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando {Count} itens de dados", request.Data.Length);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Simular processamento de cada item
        var processedCount = 0;
        foreach (var item in request.Data)
        {
            await Task.Delay(50, cancellationToken); // Simular processamento
            processedCount++;
            
            if (processedCount % 10 == 0)
            {
                _logger.LogDebug("Processados {ProcessedCount}/{TotalCount} itens", 
                    processedCount, request.Data.Length);
            }
        }

        stopwatch.Stop();

        _logger.LogInformation("Processamento concluído: {ProcessedCount} itens em {Duration}ms", 
            processedCount, stopwatch.ElapsedMilliseconds);

        return OperationResult.SuccessResult(
            $"Processados {processedCount} itens com sucesso",
            processedCount,
            stopwatch.Elapsed);
    }
}
