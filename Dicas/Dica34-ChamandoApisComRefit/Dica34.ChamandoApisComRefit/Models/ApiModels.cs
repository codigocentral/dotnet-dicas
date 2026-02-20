namespace Dica34.ChamandoApisComRefit.Models;

/// <summary>
/// Modelo de usuário retornado pelas APIs
/// </summary>
public record User(
    int Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    Address? Address = null
);

/// <summary>
/// Modelo de endereço do usuário
/// </summary>
public record Address(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    Geo? Geo = null
);

/// <summary>
/// Coordenadas geográficas
/// </summary>
public record Geo(
    double Latitude,
    double Longitude
);

/// <summary>
/// Requisição para criar um novo usuário
/// </summary>
public record CreateUserRequest(
    string Name,
    string Email,
    Address? Address = null
);

/// <summary>
/// Requisição para atualizar um usuário existente
/// </summary>
public record UpdateUserRequest(
    string Name,
    string Email,
    bool IsActive,
    Address? Address = null
);

/// <summary>
/// Filtros para busca de usuários
/// </summary>
public record UserFilter(
    string? Name = null,
    string? Email = null,
    bool? IsActive = null,
    string? City = null
);

/// <summary>
/// Resultado paginado
/// </summary>
public record PagedResult<T>(
    List<T> Data,
    PaginationInfo Pagination
);

/// <summary>
/// Informações de paginação
/// </summary>
public record PaginationInfo(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasPrevious,
    bool HasNext,
    PaginationLinks Links
);

/// <summary>
/// Links de navegação da paginação
/// </summary>
public record PaginationLinks(
    string? First = null,
    string? Previous = null,
    string? Current = null,
    string? Next = null,
    string? Last = null
);

/// <summary>
/// Modelo de produto para demonstrar diferentes APIs
/// </summary>
public record Product(
    int Id,
    string Title,
    decimal Price,
    string Description,
    string Category,
    string Image,
    ProductRating Rating
);

/// <summary>
/// Rating de produto
/// </summary>
public record ProductRating(
    double Rate,
    int Count
);

/// <summary>
/// Resultado de upload de arquivo
/// </summary>
public record UploadResult(
    string FileName,
    long FileSize,
    string ContentType,
    string Url,
    DateTime UploadedAt
);

/// <summary>
/// Dados seguros que requerem autenticação
/// </summary>
public record SecureData(
    string Id,
    string SensitiveInfo,
    DateTime AccessedAt,
    string UserId
);

/// <summary>
/// Resposta de erro padronizada da API
/// </summary>
public record ApiError(
    string Code,
    string Message,
    Dictionary<string, string[]>? ValidationErrors = null,
    string? TraceId = null
);
