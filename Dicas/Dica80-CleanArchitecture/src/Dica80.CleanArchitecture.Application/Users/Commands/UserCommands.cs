using AutoMapper;
using Dica80.CleanArchitecture.Application.Common;
using Dica80.CleanArchitecture.Application.DTOs;
using Dica80.CleanArchitecture.Domain.Entities;
using Dica80.CleanArchitecture.Domain.Repositories;
using Dica80.CleanArchitecture.Domain.ValueObjects;
using FluentValidation;

namespace Dica80.CleanArchitecture.Application.Users.Commands;

/// <summary>
/// Command to create a new user
/// </summary>
public record CreateUserCommand : BaseCommand<Result<UserDto>>
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public Domain.Enums.UserRole Role { get; init; } = Domain.Enums.UserRole.User;
}

/// <summary>
/// Validator for CreateUserCommand
/// </summary>
public class CreateUserCommandValidator : BaseValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        ValidateRequiredEmail(x => x.Email);
        ValidateRequiredString(nameof(CreateUserCommand.FirstName), x => x.FirstName);
        ValidateRequiredString(nameof(CreateUserCommand.LastName), x => x.LastName);
        ValidateEnum(nameof(CreateUserCommand.Role), x => x.Role);
    }
}

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public class CreateUserCommandHandler : BaseCommandHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user with email already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("User with this email already exists");
            }

            // Create email value object (will validate format)
            var email = Email.Create(request.Email);

            // Create user entity
            var user = User.Create(request.FirstName, request.LastName, request.Email, "TempPassword123!");

            // Add to repository
            await _userRepository.AddAsync(user);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            var userDto = Mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (ArgumentException ex)
        {
            return Result<UserDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to create user: {ex.Message}");
        }
    }
}

/// <summary>
/// Command to update an existing user
/// </summary>
public record UpdateUserCommand : BaseCommand<Result<UserDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Domain.Enums.UserRole Role { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Validator for UpdateUserCommand
/// </summary>
public class UpdateUserCommandValidator : BaseValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        ValidateRequiredId(nameof(UpdateUserCommand.Id), x => x.Id);
        ValidateRequiredString(nameof(UpdateUserCommand.Name), x => x.Name);
        ValidateEnum(nameof(UpdateUserCommand.Role), x => x.Role);
    }
}

/// <summary>
/// Handler for UpdateUserCommand
/// </summary>
public class UpdateUserCommandHandler : BaseCommandHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            // Update user profile - needs to be split for firstName/lastName
            var nameParts = request.Name.Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";
            user.UpdateProfile(firstName, lastName);
            
            if (request.IsActive != user.IsActive)
            {
                if (request.IsActive)
                    user.Activate();
                else
                    user.Deactivate();
            }

            // Update in repository
            await _userRepository.UpdateAsync(user);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            var userDto = Mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to update user: {ex.Message}");
        }
    }
}

/// <summary>
/// Command to delete a user
/// </summary>
public record DeleteUserCommand : BaseCommand<Result>
{
    public Guid Id { get; init; }
}

/// <summary>
/// Validator for DeleteUserCommand
/// </summary>
public class DeleteUserCommandValidator : BaseValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        ValidateRequiredId(nameof(DeleteUserCommand.Id), x => x.Id);
    }
}

/// <summary>
/// Handler for DeleteUserCommand
/// </summary>
public class DeleteUserCommandHandler : BaseCommandHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            // Soft delete user
            await _userRepository.DeleteAsync(user);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete user: {ex.Message}");
        }
    }
}
