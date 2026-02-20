using Dica34.ChamandoApisComRefit.Apis;
using Dica34.ChamandoApisComRefit.Models;
using Dica34.ChamandoApisComRefit.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Refit;
using System.Net;
using Xunit;

namespace Dica34.ChamandoApisComRefit.Tests;

/// <summary>
/// Testes unitários para UserService demonstrando mocking de APIs Refit
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserApi> _mockUserApi;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserApi = new Mock<IUserApi>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockUserApi.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User(
            Id: userId,
            Name: "João Silva",
            Email: "joao@test.com",
            IsActive: true,
            CreatedAt: DateTime.Now
        );

        var response = new ApiResponse<User>(
            new HttpResponseMessage(HttpStatusCode.OK),
            expectedUser,
            new RefitSettings()
        );

        _mockUserApi.Setup(x => x.GetUserWithResponseAsync(userId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(response);

        // Act
        var result = await _userService.GetUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal(expectedUser.Name, result.Name);
        Assert.Equal(expectedUser.Email, result.Email);

        _mockUserApi.Verify(x => x.GetUserWithResponseAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = 999;
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, $"/users/{userId}"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new RefitSettings()
        );

        _mockUserApi.Setup(x => x.GetUserWithResponseAsync(userId, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(apiException);

        // Act
        var result = await _userService.GetUserAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldReturnCreatedUser()
    {
        // Arrange
        var request = new CreateUserRequest(
            Name: "Maria Santos",
            Email: "maria@test.com"
        );

        var expectedUser = new User(
            Id: 123,
            Name: request.Name,
            Email: request.Email,
            IsActive: true,
            CreatedAt: DateTime.Now
        );

        _mockUserApi.Setup(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Name, result.Name);
        Assert.Equal(expectedUser.Email, result.Email);
        Assert.True(result.Id > 0);

        _mockUserApi.Verify(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithInvalidData_ShouldThrowApiException()
    {
        // Arrange
        var request = new CreateUserRequest(
            Name: "", // Nome inválido
            Email: "invalid-email"
        );

        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "/users"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"Invalid data\"}")
            },
            new RefitSettings()
        );

        _mockUserApi.Setup(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(apiException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => _userService.CreateUserAsync(request));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task SearchUsersByNameAsync_WithEmptyQuery_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyQuery = "";

        // Act
        var result = await _userService.SearchUsersByNameAsync(emptyQuery);

        // Assert
        Assert.Empty(result);
        
        // Verificar que a API não foi chamada
        _mockUserApi.Verify(x => x.SearchUsersByNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), 
                           Times.Never);
    }

    [Fact]
    public async Task SearchUsersByNameAsync_WithValidQuery_ShouldReturnUsers()
    {
        // Arrange
        var query = "João";
        var expectedUsers = new List<User>
        {
            new(1, "João Silva", "joao@test.com", true, DateTime.Now),
            new(2, "João Santos", "joao.santos@test.com", true, DateTime.Now)
        };

        _mockUserApi.Setup(x => x.SearchUsersByNameAsync(query, 10, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.SearchUsersByNameAsync(query);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, user => Assert.Contains(query, user.Name));

        _mockUserApi.Verify(x => x.SearchUsersByNameAsync(query, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var userId = 1;

        _mockUserApi.Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.True(result);
        _mockUserApi.Verify(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserNotFound_ShouldReturnFalse()
    {
        // Arrange
        var userId = 999;
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Delete, $"/users/{userId}"),
            HttpMethod.Delete,
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new RefitSettings()
        );

        _mockUserApi.Setup(x => x.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(apiException);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldReturnUpdatedUser()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserRequest(
            Name: "João Silva Atualizado",
            Email: "joao.updated@test.com",
            IsActive: false
        );

        var expectedUser = new User(
            Id: userId,
            Name: updateRequest.Name,
            Email: updateRequest.Email,
            IsActive: updateRequest.IsActive,
            CreatedAt: DateTime.Now.AddDays(-30)
        );

        _mockUserApi.Setup(x => x.UpdateUserAsync(userId, updateRequest, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Name, result.Name);
        Assert.Equal(expectedUser.Email, result.Email);
        Assert.Equal(expectedUser.IsActive, result.IsActive);

        _mockUserApi.Verify(x => x.UpdateUserAsync(userId, updateRequest, It.IsAny<CancellationToken>()), Times.Once);
    }
}

/// <summary>
/// Testes unitários para ProductService
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IProductApi> _mockProductApi;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockProductApi = new Mock<IProductApi>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_mockProductApi.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new(1, "Produto 1", 100.00m, "Descrição 1", "electronics", "image1.jpg", new ProductRating(4.5, 10)),
            new(2, "Produto 2", 200.00m, "Descrição 2", "clothing", "image2.jpg", new ProductRating(4.0, 20))
        };

        _mockProductApi.Setup(x => x.GetProductsAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedProducts);

        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(expectedProducts, result);

        _mockProductApi.Verify(x => x.GetProductsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ShouldReturnFilteredProducts()
    {
        // Arrange
        var category = "electronics";
        var expectedProducts = new List<Product>
        {
            new(1, "Laptop", 1000.00m, "Gaming laptop", category, "laptop.jpg", new ProductRating(4.8, 50)),
            new(2, "Mouse", 50.00m, "Gaming mouse", category, "mouse.jpg", new ProductRating(4.2, 30))
        };

        _mockProductApi.Setup(x => x.GetProductsByCategoryAsync(category, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedProducts);

        // Act
        var result = await _productService.GetProductsByCategoryAsync(category);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, product => Assert.Equal(category, product.Category));

        _mockProductApi.Verify(x => x.GetProductsByCategoryAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnCategories()
    {
        // Arrange
        var expectedCategories = new List<string>
        {
            "electronics",
            "clothing",
            "books",
            "home"
        };

        _mockProductApi.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedCategories);

        // Act
        var result = await _productService.GetCategoriesAsync();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal(expectedCategories, result);

        _mockProductApi.Verify(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

/// <summary>
/// Testes unitários para SecureService
/// </summary>
public class SecureServiceTests
{
    private readonly Mock<ISecureApi> _mockSecureApi;
    private readonly Mock<ILogger<SecureService>> _mockLogger;
    private readonly SecureService _secureService;

    public SecureServiceTests()
    {
        _mockSecureApi = new Mock<ISecureApi>();
        _mockLogger = new Mock<ILogger<SecureService>>();
        _secureService = new SecureService(_mockSecureApi.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetSecureDataAsync_WithValidToken_ShouldReturnData()
    {
        // Arrange
        var accessToken = "valid-token-123";
        var expectedData = new SecureData(
            Id: "data-123",
            SensitiveInfo: "Informação confidencial",
            AccessedAt: DateTime.Now,
            UserId: "user-456"
        );

        _mockSecureApi.Setup(x => x.GetSecureDataAsync($"Bearer {accessToken}", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedData);

        // Act
        var result = await _secureService.GetSecureDataAsync(accessToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedData.Id, result.Id);
        Assert.Equal(expectedData.SensitiveInfo, result.SensitiveInfo);

        _mockSecureApi.Verify(x => x.GetSecureDataAsync($"Bearer {accessToken}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSecureDataAsync_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid-token";
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "/secure/data"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new RefitSettings()
        );

        _mockSecureApi.Setup(x => x.GetSecureDataAsync($"Bearer {invalidToken}", It.IsAny<CancellationToken>()))
                     .ThrowsAsync(apiException);

        // Act
        var result = await _secureService.GetSecureDataAsync(invalidToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSecureDataAsync_WithBearerPrefixedToken_ShouldUseTokenDirectly()
    {
        // Arrange
        var bearerToken = "Bearer existing-bearer-token";
        var expectedData = new SecureData(
            Id: "data-789",
            SensitiveInfo: "Dados secretos",
            AccessedAt: DateTime.Now,
            UserId: "user-123"
        );

        _mockSecureApi.Setup(x => x.GetSecureDataAsync(bearerToken, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedData);

        // Act
        var result = await _secureService.GetSecureDataAsync(bearerToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedData.Id, result.Id);

        // Verificar que não adicionou Bearer prefix duplicado
        _mockSecureApi.Verify(x => x.GetSecureDataAsync(bearerToken, It.IsAny<CancellationToken>()), Times.Once);
    }
}
