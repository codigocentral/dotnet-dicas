using Dica34.ChamandoApisComRefit.Apis;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Dica34.ChamandoApisComRefit.Tests;

/// <summary>
/// Testes de integração usando WireMock para simular APIs reais
/// </summary>
public class IntegrationTests : IDisposable
{
    private readonly WireMockServer _mockServer;
    private readonly IUserApi _userApi;
    private readonly IProductApi _productApi;

    public IntegrationTests()
    {
        // Configurar servidor mock
        _mockServer = WireMockServer.Start();

        // Configurar serviços
        var services = new ServiceCollection();
        
        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            })
        };

        services.AddRefitClient<IUserApi>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(_mockServer.Urls[0]));

        services.AddRefitClient<IProductApi>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(_mockServer.Urls[0]));

        var serviceProvider = services.BuildServiceProvider();
        
        _userApi = serviceProvider.GetRequiredService<IUserApi>();
        _productApi = serviceProvider.GetRequiredService<IProductApi>();
    }

    [Fact]
    public async Task GetUserAsync_Integration_ShouldReturnUserFromMockApi()
    {
        // Arrange
        var userId = 1;
        var mockUser = new
        {
            id = userId,
            name = "João da Silva",
            email = "joao@integration-test.com",
            isActive = true,
            createdAt = DateTime.Now
        };

        _mockServer
            .Given(Request.Create()
                .WithPath($"/users/{userId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(mockUser));

        // Act
        var response = await _userApi.GetUserWithResponseAsync(userId);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(response.Content);
        Assert.Equal(mockUser.name, response.Content.Name);
        Assert.Equal(mockUser.email, response.Content.Email);
        Assert.Equal(mockUser.isActive, response.Content.IsActive);
    }

    [Fact]
    public async Task GetUserAsync_WhenNotFound_ShouldThrowApiException()
    {
        // Arrange
        var userId = 999;

        _mockServer
            .Given(Request.Create()
                .WithPath($"/users/{userId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "User not found" }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => _userApi.GetUserAsync(userId));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task CreateUserAsync_Integration_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var createRequest = new
        {
            name = "Maria Santos",
            email = "maria@integration-test.com"
        };

        var createdUser = new
        {
            id = 123,
            name = createRequest.name,
            email = createRequest.email,
            isActive = true,
            createdAt = DateTime.Now
        };

        _mockServer
            .Given(Request.Create()
                .WithPath("/users")
                .UsingPost()
                .WithBodyAsJson(createRequest))
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(createdUser));

        // Act
        var request = new Dica34.ChamandoApisComRefit.Models.CreateUserRequest(
            createRequest.name, 
            createRequest.email
        );
        
        var result = await _userApi.CreateUserAsync(request);

        // Assert
        Assert.Equal(createdUser.id, result.Id);
        Assert.Equal(createdUser.name, result.Name);
        Assert.Equal(createdUser.email, result.Email);
        Assert.Equal(createdUser.isActive, result.IsActive);
    }

    [Fact]
    public async Task GetProductsAsync_Integration_ShouldReturnProductList()
    {
        // Arrange
        var mockProducts = new[]
        {
            new
            {
                id = 1,
                title = "Laptop Gamer",
                price = 2499.99,
                description = "Laptop para jogos",
                category = "electronics",
                image = "laptop.jpg",
                rating = new { rate = 4.5, count = 25 }
            },
            new
            {
                id = 2,
                title = "Mouse Gaming",
                price = 149.99,
                description = "Mouse para jogos",
                category = "electronics",
                image = "mouse.jpg",
                rating = new { rate = 4.2, count = 15 }
            }
        };

        _mockServer
            .Given(Request.Create()
                .WithPath("/products")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(mockProducts));

        // Act
        var result = await _productApi.GetProductsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(mockProducts[0].title, result[0].Title);
        Assert.Equal((decimal)mockProducts[0].price, result[0].Price);
        Assert.Equal(mockProducts[1].title, result[1].Title);
        Assert.Equal((decimal)mockProducts[1].price, result[1].Price);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_Integration_ShouldFilterByCategory()
    {
        // Arrange
        var category = "electronics";
        var mockProducts = new[]
        {
            new
            {
                id = 1,
                title = "Smartphone",
                price = 899.99,
                description = "Smartphone avançado",
                category = category,
                image = "phone.jpg",
                rating = new { rate = 4.8, count = 100 }
            }
        };

        _mockServer
            .Given(Request.Create()
                .WithPath($"/products/category/{category}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(mockProducts));

        // Act
        var result = await _productApi.GetProductsByCategoryAsync(category);

        // Assert
        Assert.Single(result);
        Assert.Equal(category, result[0].Category);
        Assert.Equal(mockProducts[0].title, result[0].Title);
    }

    [Fact]
    public async Task GetCategoriesAsync_Integration_ShouldReturnCategoryList()
    {
        // Arrange
        var mockCategories = new[]
        {
            "electronics",
            "clothing",
            "books",
            "home & garden"
        };

        _mockServer
            .Given(Request.Create()
                .WithPath("/products/categories")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(mockCategories));

        // Act
        var result = await _productApi.GetCategoriesAsync();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal(mockCategories, result);
    }

    [Fact]
    public async Task UpdateUserAsync_Integration_ShouldUpdateUserSuccessfully()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new
        {
            name = "João Silva Atualizado",
            email = "joao.updated@test.com",
            isActive = false
        };

        var updatedUser = new
        {
            id = userId,
            name = updateRequest.name,
            email = updateRequest.email,
            isActive = updateRequest.isActive,
            createdAt = DateTime.Now.AddDays(-30)
        };

        _mockServer
            .Given(Request.Create()
                .WithPath($"/users/{userId}")
                .UsingPut()
                .WithBodyAsJson(updateRequest))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(updatedUser));

        // Act
        var request = new Dica34.ChamandoApisComRefit.Models.UpdateUserRequest(
            updateRequest.name,
            updateRequest.email,
            updateRequest.isActive
        );

        var result = await _userApi.UpdateUserAsync(userId, request);

        // Assert
        Assert.Equal(updatedUser.id, result.Id);
        Assert.Equal(updatedUser.name, result.Name);
        Assert.Equal(updatedUser.email, result.Email);
        Assert.Equal(updatedUser.isActive, result.IsActive);
    }

    [Fact]
    public async Task DeleteUserAsync_Integration_ShouldDeleteSuccessfully()
    {
        // Arrange
        var userId = 1;

        _mockServer
            .Given(Request.Create()
                .WithPath($"/users/{userId}")
                .UsingDelete())
            .RespondWith(Response.Create()
                .WithStatusCode(204)); // No Content

        // Act & Assert
        // Não deve lançar exceção
        await _userApi.DeleteUserAsync(userId);

        // Verificar que a requisição foi feita
        var requests = _mockServer.LogEntries;
        Assert.Contains(requests, log => 
            log.RequestMessage.Path == $"/users/{userId}" && 
            log.RequestMessage.Method == "DELETE");
    }

    [Fact]
    public async Task SearchUsersByNameAsync_Integration_ShouldReturnFilteredUsers()
    {
        // Arrange
        var searchQuery = "João";
        var limit = 5;
        var mockUsers = new[]
        {
            new
            {
                id = 1,
                name = "João Silva",
                email = "joao.silva@test.com",
                isActive = true,
                createdAt = DateTime.Now
            },
            new
            {
                id = 2,
                name = "João Santos",
                email = "joao.santos@test.com",
                isActive = true,
                createdAt = DateTime.Now
            }
        };

        _mockServer
            .Given(Request.Create()
                .WithPath("/users/search")
                .WithParam("q", searchQuery)
                .WithParam("limit", limit.ToString())
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(mockUsers));

        // Act
        var result = await _userApi.SearchUsersByNameAsync(searchQuery, limit);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, user => Assert.Contains(searchQuery, user.Name));
    }

    [Fact]
    public async Task ApiExceptionHandling_Integration_ShouldProvideDetailedErrorInfo()
    {
        // Arrange
        var userId = 500; // Simular erro interno do servidor
        var errorResponse = new
        {
            error = "Internal server error",
            message = "Something went wrong",
            traceId = Guid.NewGuid().ToString()
        };

        _mockServer
            .Given(Request.Create()
                .WithPath($"/users/{userId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(500)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(errorResponse));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => _userApi.GetUserAsync(userId));

        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, exception.StatusCode);
        Assert.Contains("Internal server error", exception.Content);
    }

    public void Dispose()
    {
        _mockServer?.Stop();
        _mockServer?.Dispose();
    }
}
