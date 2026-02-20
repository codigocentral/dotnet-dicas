using Dica10.MarcadoresAssembly.Models;

namespace Dica10.MarcadoresAssembly.Profiles;

/// <summary>
/// Profile do AutoMapper para mapeamento entre User e DTOs
/// </summary>
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<User, UserDisplayDto>()
            .ForMember(dest => dest.FormattedCreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd/MM/yyyy HH:mm")))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Ativo" : "Inativo"));
    }
}
