using AutoMapper;
using Dica80.CleanArchitecture.Application.Common;
using Dica80.CleanArchitecture.Application.DTOs;
using Dica80.CleanArchitecture.Domain.Entities;
using Dica80.CleanArchitecture.Domain.Repositories;
using Dica80.CleanArchitecture.Domain.ValueObjects;
using FluentValidation;

namespace Dica80.CleanArchitecture.Application.Projects.Commands;

/// <summary>
/// Command to create a new project
/// </summary>
public record CreateProjectCommand : BaseCommand<Result<ProjectDto>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid OwnerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
}

/// <summary>
/// Validator for CreateProjectCommand
/// </summary>
public class CreateProjectCommandValidator : BaseValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        ValidateRequiredString(nameof(CreateProjectCommand.Name), x => x.Name);
        ValidateDescription(x => x.Description);
        ValidateRequiredId(nameof(CreateProjectCommand.OwnerId), x => x.OwnerId);

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        When(x => x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate!.Value)
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be after start date");
        });

        When(x => x.BudgetAmount.HasValue, () =>
        {
            ValidatePositiveDecimal(nameof(CreateProjectCommand.BudgetAmount), x => x.BudgetAmount!.Value);

            RuleFor(x => x.BudgetCurrency)
                .NotEmpty()
                .WithMessage("Budget currency is required when budget amount is specified")
                .Length(3)
                .WithMessage("Currency must be 3 characters (e.g., USD, EUR)");
        });
    }
}

/// <summary>
/// Handler for CreateProjectCommand
/// </summary>
public class CreateProjectCommandHandler : BaseCommandHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(mapper)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify owner exists
            var owner = await _userRepository.GetByIdAsync(request.OwnerId);
            if (owner == null)
            {
                return Result<ProjectDto>.Failure("Owner not found");
            }

            if (!owner.IsActive)
            {
                return Result<ProjectDto>.Failure("Owner is not active");
            }

            // Create budget if provided
            Money? budget = null;
            if (request.BudgetAmount.HasValue && !string.IsNullOrEmpty(request.BudgetCurrency))
            {
                budget = Money.Create(request.BudgetAmount.Value, request.BudgetCurrency);
            }

            // Create project
            var project = Project.Create(
                request.Name,
                request.Description,
                request.OwnerId,
                budget);

            // Add to repository
            await _projectRepository.AddAsync(project);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            var projectDto = Mapper.Map<ProjectDto>(project);
            return Result<ProjectDto>.Success(projectDto);
        }
        catch (ArgumentException ex)
        {
            return Result<ProjectDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProjectDto>.Failure($"Failed to create project: {ex.Message}");
        }
    }
}

/// <summary>
/// Command to complete a project
/// </summary>
public record CompleteProjectCommand : BaseCommand<Result>
{
    public Guid ProjectId { get; init; }
}

/// <summary>
/// Validator for CompleteProjectCommand
/// </summary>
public class CompleteProjectCommandValidator : BaseValidator<CompleteProjectCommand>
{
    public CompleteProjectCommandValidator()
    {
        ValidateRequiredId(nameof(CompleteProjectCommand.ProjectId), x => x.ProjectId);
    }
}

/// <summary>
/// Handler for CompleteProjectCommand
/// </summary>
public class CompleteProjectCommandHandler : BaseCommandHandler<CompleteProjectCommand, Result>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(mapper)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(CompleteProjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
            {
                return Result.Failure("Project not found");
            }

            // Complete project (this will validate business rules and raise domain events)
            project.Complete();

            // Update in repository
            await _projectRepository.UpdateAsync(project);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to complete project: {ex.Message}");
        }
    }
}
