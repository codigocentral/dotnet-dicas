using AutoMapper;
using Dica80.CleanArchitecture.Application.DTOs;
using Dica80.CleanArchitecture.Application.Users.Queries;
using Dica80.CleanArchitecture.Domain.Entities;

namespace Dica80.CleanArchitecture.Application.Mappings;

/// <summary>
/// AutoMapper profile for application mappings
/// </summary>
public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateUserMappings();
        CreateProjectMappings();
        CreateTaskMappings();
        CreateCommentMappings();
    }

    private void CreateUserMappings()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));

        CreateMap<CreateUserDto, User>()
            .ConstructUsing(src => User.Create(
                src.FirstName,
                src.LastName,
                src.Email,
                "TempPassword123!")); // Temporary password, should be handled differently

        // User stats mapping
        CreateMap<object, UserStatsDto>(); // This would be mapped from repository query results
    }

    private void CreateProjectMappings()
    {
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.FullName : string.Empty))
            .ForMember(dest => dest.Budget, opt => opt.MapFrom(src => src.Budget != null ? src.Budget.Amount : (decimal?)null))
            .ForMember(dest => dest.BudgetCurrency, opt => opt.MapFrom(src => src.Budget != null ? src.Budget.Currency : null))
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count))
            .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done)));

        CreateMap<CreateProjectDto, Project>()
            .ConstructUsing(src => Project.Create(
                src.Name,
                src.Description,
                src.OwnerId,
                src.BudgetAmount.HasValue && !string.IsNullOrEmpty(src.BudgetCurrency)
                    ? Domain.ValueObjects.Money.Create(src.BudgetAmount.Value, src.BudgetCurrency)
                    : null));
    }

    private void CreateTaskMappings()
    {
        CreateMap<TaskItem, TaskDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.Assignee != null ? src.Assignee.FullName : null))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));

        CreateMap<CreateTaskDto, TaskItem>()
            .ConstructUsing(src => TaskItem.Create(
                src.Title,
                src.Description,
                src.Priority,
                src.ProjectId,
                src.AssignedToId));
    }

    private void CreateCommentMappings()
    {
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task != null ? src.Task.Title : string.Empty))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.FullName : string.Empty));

        CreateMap<CreateCommentDto, Comment>()
            .ConstructUsing(src => Comment.Create(
                src.Content,
                src.TaskId,
                src.AuthorId));
    }
}

/// <summary>
/// Extension methods for AutoMapper configuration
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps a source object to destination type with null safety
    /// </summary>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="source">Source object</param>
    /// <returns>Mapped object or default if source is null</returns>
    public static TDestination? MapOrDefault<TDestination>(this IMapper mapper, object? source)
        where TDestination : class
    {
        return source == null ? default : mapper.Map<TDestination>(source);
    }

    /// <summary>
    /// Maps a collection with null safety
    /// </summary>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="source">Source collection</param>
    /// <returns>Mapped collection or empty list if source is null</returns>
    public static List<TDestination> MapOrEmpty<TDestination>(this IMapper mapper, IEnumerable<object>? source)
    {
        return source == null ? new List<TDestination>() : mapper.Map<List<TDestination>>(source);
    }
}
