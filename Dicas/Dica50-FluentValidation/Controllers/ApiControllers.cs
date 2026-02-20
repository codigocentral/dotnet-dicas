using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Dica50.FluentValidation.Models;
using Dica50.FluentValidation.Services;

namespace Dica50.FluentValidation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserRequest> _createUserValidator;
    private readonly IValidator<UpdateUserRequest> _updateUserValidator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserRequest> createUserValidator,
        IValidator<UpdateUserRequest> updateUserValidator,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
        _logger = logger;
    }

    /// <summary>
    /// Criar um novo usuário com validação FluentValidation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Recebida solicitação para criar usuário: {UserName}", request.Name);

        // Validação manual (demonstração)
        var validationResult = await _createUserValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para criação de usuário: {Errors}", 
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new
            {
                Message = "Dados inválidos",
                Errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage,
                    AttemptedValue = e.AttemptedValue
                })
            });
        }

        try
        {
            var user = await _userService.CreateUserAsync(request);
            _logger.LogInformation("Usuário criado com sucesso: ID {UserId}", user.Id);
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário");
            return StatusCode(500, new { Message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualizar usuário existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Recebida solicitação para atualizar usuário ID: {UserId}", id);

        // Validação manual (demonstração)
        var validationResult = await _updateUserValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para atualização de usuário ID {UserId}: {Errors}", 
                id, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new
            {
                Message = "Dados inválidos",
                Errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage,
                    AttemptedValue = e.AttemptedValue
                })
            });
        }

        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            _logger.LogInformation("Usuário ID {UserId} atualizado com sucesso", id);
            
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Usuário não encontrado: {Message}", ex.Message);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário ID {UserId}", id);
            return StatusCode(500, new { Message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Buscar usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        _logger.LogInformation("Buscando usuário ID: {UserId}", id);

        if (id <= 0)
        {
            return BadRequest(new { Message = "ID deve ser maior que zero" });
        }

        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogInformation("Usuário ID {UserId} não encontrado", id);
                return NotFound(new { Message = $"Usuário com ID {id} não encontrado" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário ID {UserId}", id);
            return StatusCode(500, new { Message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Listar todos os usuários
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAllUsers()
    {
        _logger.LogInformation("Listando todos os usuários");

        try
        {
            var users = await _userService.GetAllUsersAsync();
            _logger.LogInformation("Retornando {UserCount} usuários", users.Count);
            
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return StatusCode(500, new { Message = "Erro interno do servidor" });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<Product> _productValidator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IValidator<Product> productValidator,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _productValidator = productValidator;
        _logger = logger;
    }

    /// <summary>
    /// Criar um novo produto com validação complexa
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        _logger.LogInformation("Recebida solicitação para criar produto: {ProductName}", product.Name);

        // Validação com FluentValidation
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para criação de produto: {Errors}", 
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new
            {
                Message = "Dados do produto inválidos",
                Errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
            });
        }

        try
        {
            var createdProduct = await _productService.CreateProductAsync(product);
            _logger.LogInformation("Produto criado com sucesso: ID {ProductId}", createdProduct.Id);
            
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto");
            return StatusCode(500, new { Message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Buscar produto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        _logger.LogInformation("Buscando produto ID: {ProductId}", id);

        if (id <= 0)
        {
            return BadRequest(new { Message = "ID deve ser maior que zero" });
        }

        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogInformation("Produto ID {ProductId} não encontrado", id);
                return NotFound(new { Message = $"Produto com ID {id} não encontrado" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto ID {ProductId}", id);
            return StatusCode(500, new { Message = "Erro interno do servidor" });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly IValidator<Company> _companyValidator;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(
        IValidator<Company> companyValidator,
        ILogger<CompaniesController> logger)
    {
        _companyValidator = companyValidator;
        _logger = logger;
    }

    /// <summary>
    /// Validar dados de empresa (demonstração de validação assíncrona)
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult> ValidateCompany([FromBody] Company company)
    {
        _logger.LogInformation("Validando dados da empresa: {CompanyName}", company.Name);

        try
        {
            // Demonstração de validação assíncrona
            var validationResult = await _companyValidator.ValidateAsync(company);
            
            if (validationResult.IsValid)
            {
                _logger.LogInformation("Dados da empresa {CompanyName} são válidos", company.Name);
                return Ok(new { Message = "Dados da empresa são válidos", IsValid = true });
            }

            _logger.LogWarning("Validação falhou para empresa {CompanyName}: {Errors}", 
                company.Name, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new
            {
                Message = "Dados da empresa inválidos",
                IsValid = false,
                Errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage,
                    AttemptedValue = e.AttemptedValue,
                    Severity = e.Severity.ToString()
                }).ToArray()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar empresa");
            return StatusCode(500, new { Message = "Erro interno do servidor" });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IValidator<Order> _orderValidator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IValidator<Order> orderValidator,
        ILogger<OrdersController> logger)
    {
        _orderValidator = orderValidator;
        _logger = logger;
    }

    /// <summary>
    /// Validar pedido (demonstração de validações complexas)
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult> ValidateOrder([FromBody] Order order)
    {
        _logger.LogInformation("Validando pedido: {OrderNumber}", order.OrderNumber);

        try
        {
            var validationResult = await _orderValidator.ValidateAsync(order);
            
            if (validationResult.IsValid)
            {
                _logger.LogInformation("Pedido {OrderNumber} é válido", order.OrderNumber);
                
                // Calcular valores para resposta
                var subtotal = order.Items.Sum(item => (item.UnitPrice * item.Quantity) - item.Discount);
                var total = subtotal + order.ShippingCost - order.Discount;
                
                return Ok(new 
                { 
                    Message = "Pedido válido",
                    IsValid = true,
                    Summary = new
                    {
                        ItemCount = order.Items.Count,
                        Subtotal = subtotal,
                        ShippingCost = order.ShippingCost,
                        Discount = order.Discount,
                        Total = total
                    }
                });
            }

            _logger.LogWarning("Validação falhou para pedido {OrderNumber}: {Errors}", 
                order.OrderNumber, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new
            {
                Message = "Pedido inválido",
                IsValid = false,
                Errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => string.IsNullOrEmpty(g.Key) ? "General" : g.Key,
                        g => g.Select(e => new
                        {
                            Message = e.ErrorMessage,
                            AttemptedValue = e.AttemptedValue,
                            Severity = e.Severity.ToString()
                        }).ToArray()
                    )
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar pedido");
            return StatusCode(500, new { Message = "Erro interno do servidor" });
        }
    }
}
