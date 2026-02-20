using Dica34.ChamandoApisComRefit.Models;
using Refit;

namespace Dica34.ChamandoApisComRefit.Apis;

/// <summary>
/// Interface para API de usuários - demonstra operações CRUD básicas
/// </summary>
public interface IUserApi
{
    /// <summary>
    /// Obtém todos os usuários com paginação e filtros
    /// </summary>
    [Get("/users")]
    Task<PagedResult<User>> GetUsersAsync(
        int page = 1,
        int size = 10,
        string? search = null,
        [Query] UserFilter? filter = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Obtém um usuário específico por ID
    /// </summary>
    [Get("/users/{id}")]
    Task<User> GetUserAsync(
        int id, 
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Obtém um usuário com informações de resposta HTTP
    /// </summary>
    [Get("/users/{id}")]
    Task<ApiResponse<User>> GetUserWithResponseAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    [Post("/users")]
    Task<User> CreateUserAsync(
        [Body] CreateUserRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    [Put("/users/{id}")]
    Task<User> UpdateUserAsync(
        int id,
        [Body] UpdateUserRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Atualização parcial do usuário usando PATCH
    /// </summary>
    [Patch("/users/{id}")]
    Task<User> PatchUserAsync(
        int id,
        [Body] Dictionary<string, object> updates,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Remove um usuário
    /// </summary>
    [Delete("/users/{id}")]
    Task DeleteUserAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Busca usuários por nome - demonstra query string personalizada
    /// </summary>
    [Get("/users/search")]
    Task<List<User>> SearchUsersByNameAsync(
        [Query] string q,
        [Query] int limit = 10,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Demonstra headers customizados
    /// </summary>
    [Get("/users/{id}/profile")]
    Task<User> GetUserProfileAsync(
        int id,
        [Header("X-Client-Version")] string clientVersion,
        [Header("X-Request-Id")] string requestId,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Interface para API de produtos - demonstra integração com API externa
/// </summary>
public interface IProductApi
{
    /// <summary>
    /// Obtém todos os produtos
    /// </summary>
    [Get("/products")]
    Task<List<Product>> GetProductsAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Obtém produtos por categoria
    /// </summary>
    [Get("/products/category/{category}")]
    Task<List<Product>> GetProductsByCategoryAsync(
        string category,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Obtém um produto específico
    /// </summary>
    [Get("/products/{id}")]
    Task<Product> GetProductAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Obtém todas as categorias disponíveis
    /// </summary>
    [Get("/products/categories")]
    Task<List<string>> GetCategoriesAsync(
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Interface para operações que requerem autenticação
/// </summary>
public interface ISecureApi
{
    /// <summary>
    /// Obtém dados seguros usando Bearer token
    /// </summary>
    [Get("/secure/data")]
    [Headers("Authorization: Bearer")]
    Task<SecureData> GetSecureDataAsync(
        [Header("Authorization")] string token,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Demonstra autenticação com API Key
    /// </summary>
    [Get("/api/protected")]
    Task<SecureData> GetProtectedDataAsync(
        [Header("X-API-Key")] string apiKey,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Upload de arquivo com multipart
    /// </summary>
    /*
    [Multipart]
    [Post("/upload")]
    Task<UploadResult> UploadFileAsync(
        [AliasAs("file")] StreamPart file,
        [AliasAs("description")] string description,
        [Header("Authorization")] string token,
        CancellationToken cancellationToken = default
    );
    */

    /// <summary>
    /// Download de arquivo
    /// </summary>
    [Get("/files/{fileId}")]
    Task<HttpResponseMessage> DownloadFileAsync(
        string fileId,
        [Header("Authorization")] string token,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Interface para demonstrar diferentes tipos de resposta
/// </summary>
public interface IResponseTypesApi
{
    /// <summary>
    /// Endpoint que retorna apenas status code
    /// </summary>
    [Post("/users/{id}/activate")]
    Task<HttpResponseMessage> ActivateUserAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Endpoint que pode falhar - demonstra tratamento de erro
    /// </summary>
    [Get("/users/{id}/risky")]
    Task<ApiResponse<User>> GetRiskyUserDataAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Endpoint que retorna texto puro
    /// </summary>
    [Get("/health")]
    Task<string> GetHealthCheckAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Endpoint que retorna stream de dados
    /// </summary>
    [Get("/reports/{reportId}/export")]
    Task<Stream> ExportReportAsync(
        string reportId,
        [Query] string format = "csv",
        CancellationToken cancellationToken = default
    );
}
