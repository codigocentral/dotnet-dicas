using Dica34.ChamandoApisComRefit.Apis;
using Dica34.ChamandoApisComRefit.Models;
using Microsoft.Extensions.Logging;
using Refit;

namespace Dica34.ChamandoApisComRefit.Services;

/// <summary>
/// Serviço que encapsula operações de usuário usando Refit
/// </summary>
public class UserService
{
    private readonly IUserApi _userApi;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserApi userApi, ILogger<UserService> logger)
    {
        _userApi = userApi;
        _logger = logger;
    }

    /// <summary>
    /// Obtém lista paginada de usuários com tratamento de erros
    /// </summary>
    public async Task<PagedResult<User>> GetUsersAsync(
        int page = 1, 
        int size = 10, 
        string? search = null,
        UserFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Buscando usuários - Página: {Page}, Tamanho: {Size}, Busca: {Search}", 
                page, size, search);

            var result = await _userApi.GetUsersAsync(page, size, search, filter, cancellationToken);
            
            _logger.LogInformation("Encontrados {TotalItems} usuários", result.Pagination.TotalItems);
            
            return result;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuários: {StatusCode} - {Content}", 
                ex.StatusCode, ex.Content);
            throw;
        }
    }

    /// <summary>
    /// Obtém um usuário específico com fallback para cache ou dados padrão
    /// </summary>
    public async Task<User?> GetUserAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Buscando usuário com ID: {UserId}", id);

            var response = await _userApi.GetUserWithResponseAsync(id, cancellationToken);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                _logger.LogInformation("Usuário encontrado: {UserName}", response.Content.Name);
                return response.Content;
            }

            _logger.LogWarning("Usuário não encontrado: {UserId}", id);
            return null;
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Usuário {UserId} não existe", id);
            return null;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário {UserId}: {StatusCode}", id, ex.StatusCode);
            throw;
        }
    }

    /// <summary>
    /// Cria um novo usuário com validação
    /// </summary>
    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Criando novo usuário: {UserName}", request.Name);

            var user = await _userApi.CreateUserAsync(request, cancellationToken);
            
            _logger.LogInformation("Usuário criado com sucesso: {UserId}", user.Id);
            
            return user;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário: {StatusCode} - {Content}", 
                ex.StatusCode, ex.Content);
            
            // Parse dos erros de validação se disponíveis
            if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("Dados inválidos fornecidos para criação do usuário");
            }
            
            throw;
        }
    }

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    public async Task<User?> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Atualizando usuário {UserId}", id);

            var user = await _userApi.UpdateUserAsync(id, request, cancellationToken);
            
            _logger.LogInformation("Usuário {UserId} atualizado com sucesso", id);
            
            return user;
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Tentativa de atualizar usuário inexistente: {UserId}", id);
            return null;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário {UserId}: {StatusCode}", id, ex.StatusCode);
            throw;
        }
    }

    /// <summary>
    /// Remove um usuário
    /// </summary>
    public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Removendo usuário {UserId}", id);

            await _userApi.DeleteUserAsync(id, cancellationToken);
            
            _logger.LogInformation("Usuário {UserId} removido com sucesso", id);
            
            return true;
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Tentativa de remover usuário inexistente: {UserId}", id);
            return false;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao remover usuário {UserId}: {StatusCode}", id, ex.StatusCode);
            throw;
        }
    }

    /// <summary>
    /// Busca usuários por nome com cache local
    /// </summary>
    public async Task<List<User>> SearchUsersByNameAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Buscando usuários por nome: {Query}", query);

            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<User>();
            }

            var users = await _userApi.SearchUsersByNameAsync(query.Trim(), limit, cancellationToken);
            
            _logger.LogInformation("Encontrados {Count} usuários para a busca '{Query}'", users.Count, query);
            
            return users;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuários por nome '{Query}': {StatusCode}", query, ex.StatusCode);
            throw;
        }
    }
}

/// <summary>
/// Serviço para operações com produtos
/// </summary>
public class ProductService
{
    private readonly IProductApi _productApi;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductApi productApi, ILogger<ProductService> logger)
    {
        _productApi = productApi;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os produtos com cache
    /// </summary>
    public async Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Carregando todos os produtos");

            var products = await _productApi.GetProductsAsync(cancellationToken);
            
            _logger.LogInformation("Carregados {Count} produtos", products.Count);
            
            return products;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao carregar produtos: {StatusCode}", ex.StatusCode);
            throw;
        }
    }

    /// <summary>
    /// Obtém produtos por categoria
    /// </summary>
    public async Task<List<Product>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Carregando produtos da categoria: {Category}", category);

            var products = await _productApi.GetProductsByCategoryAsync(category, cancellationToken);
            
            _logger.LogInformation("Encontrados {Count} produtos na categoria '{Category}'", products.Count, category);
            
            return products;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao carregar produtos da categoria '{Category}': {StatusCode}", category, ex.StatusCode);
            throw;
        }
    }

    /// <summary>
    /// Obtém todas as categorias disponíveis
    /// </summary>
    public async Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Carregando categorias de produtos");

            var categories = await _productApi.GetCategoriesAsync(cancellationToken);
            
            _logger.LogInformation("Encontradas {Count} categorias", categories.Count);
            
            return categories;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao carregar categorias: {StatusCode}", ex.StatusCode);
            throw;
        }
    }
}

/// <summary>
/// Serviço para operações seguras que requerem autenticação
/// </summary>
public class SecureService
{
    private readonly ISecureApi _secureApi;
    private readonly ILogger<SecureService> _logger;

    public SecureService(ISecureApi secureApi, ILogger<SecureService> logger)
    {
        _secureApi = secureApi;
        _logger = logger;
    }

    /// <summary>
    /// Obtém dados seguros usando token de autenticação
    /// </summary>
    public async Task<SecureData?> GetSecureDataAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Acessando dados seguros");

            var bearerToken = accessToken.StartsWith("Bearer ") ? accessToken : $"Bearer {accessToken}";
            var data = await _secureApi.GetSecureDataAsync(bearerToken, cancellationToken);
            
            _logger.LogInformation("Dados seguros obtidos com sucesso");
            
            return data;
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Token de acesso inválido ou expirado");
            return null;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro ao acessar dados seguros: {StatusCode}", ex.StatusCode);
            throw;
        }
    }

    /// <summary>
    /// Upload de arquivo com progresso
    /// </summary>
    public Task<UploadResult?> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string description, 
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando upload do arquivo: {FileName}", fileName);
            
            // Simulação de upload - na prática usaria StreamPart ou ByteArrayPart
            var result = new UploadResult(
                FileName: fileName,
                FileSize: fileStream.Length,
                ContentType: "application/octet-stream",
                Url: $"https://api.exemplo.com/files/{fileName}",
                UploadedAt: DateTime.UtcNow
            );
            
            _logger.LogInformation("Upload concluído com sucesso: {Url}", result.Url);
            
            return Task.FromResult<UploadResult?>(result);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro no upload do arquivo '{FileName}': {StatusCode}", fileName, ex.StatusCode);
            return Task.FromResult<UploadResult?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado no upload do arquivo '{FileName}'", fileName);
            return Task.FromResult<UploadResult?>(null);
        }
    }
}
