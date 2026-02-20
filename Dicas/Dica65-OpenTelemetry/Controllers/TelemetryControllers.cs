using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Dica65.OpenTelemetry.Services;
using Dica65.OpenTelemetry.Telemetry;

namespace Dica65.OpenTelemetry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly BusinessService _businessService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        DatabaseService databaseService,
        BusinessService businessService,
        ILogger<UsersController> logger)
    {
        _databaseService = databaseService;
        _businessService = businessService;
        _logger = logger;
    }

    /// <summary>
    /// Busca usuários com telemetria automática
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers([FromQuery] int count = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Endpoint GET /api/users chamado com count={Count}", count);
            
            // Adicionar tags customizadas à atividade atual
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("users.count_requested", count);
                Activity.Current.SetTag("endpoint.type", "list");
            }
            
            var users = await _databaseService.GetUsersAsync(count);
            
            stopwatch.Stop();
            
            // Registrar métrica customizada
            ApplicationTelemetry.RecordHttpRequest("GET", "/api/users", 200, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("users.count_returned", users.Count);
                Activity.Current.SetStatus(ActivityStatusCode.Ok);
            }
            
            return Ok(users);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("GET", "/api/users", 500, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("error.type", ex.GetType().Name);
                Activity.Current.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            
            _logger.LogError(ex, "Erro ao buscar usuários");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Cria usuário com processo de negócio completo
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Endpoint POST /api/users chamado para email={Email}", request.Email);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("user.email", request.Email);
                Activity.Current.SetTag("user.name", request.Name);
                Activity.Current.SetTag("endpoint.type", "create");
            }
            
            // Usar serviço de negócio que inclui validações externas
            var result = await _businessService.RegisterUserAsync(request);
            
            stopwatch.Stop();
            
            if (result.Success && result.User != null)
            {
                ApplicationTelemetry.RecordHttpRequest("POST", "/api/users", 201, stopwatch.ElapsedMilliseconds);
                
                if (Activity.Current != null)
                {
                    Activity.Current.SetTag("user.id", result.User.Id);
                    Activity.Current.SetStatus(ActivityStatusCode.Ok);
                }
                
                return CreatedAtAction(nameof(GetUser), new { id = result.User.Id }, result.User);
            }
            else
            {
                ApplicationTelemetry.RecordHttpRequest("POST", "/api/users", 400, stopwatch.ElapsedMilliseconds);
                
                if (Activity.Current != null)
                {
                    Activity.Current.SetTag("validation.failed", true);
                    Activity.Current.SetStatus(ActivityStatusCode.Error, result.ErrorMessage);
                }
                
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("POST", "/api/users", 500, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("error.type", ex.GetType().Name);
                Activity.Current.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            
            _logger.LogError(ex, "Erro ao criar usuário");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Busca usuário específico (simulado)
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Endpoint GET /api/users/{Id} chamado", id);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("user.id", id);
                Activity.Current.SetTag("endpoint.type", "get_single");
            }
            
            // Simular busca
            await Task.Delay(Random.Shared.Next(50, 150));
            
            // Simular usuário não encontrado
            if (Random.Shared.Next(1, 11) <= 2) // 20% de chance
            {
                stopwatch.Stop();
                ApplicationTelemetry.RecordHttpRequest("GET", $"/api/users/{id}", 404, stopwatch.ElapsedMilliseconds);
                
                if (Activity.Current != null)
                {
                    Activity.Current.SetTag("user.found", false);
                    Activity.Current.SetStatus(ActivityStatusCode.Ok, "User not found");
                }
                
                return NotFound(new { error = $"User {id} not found" });
            }
            
            var user = new UserDto
            {
                Id = id,
                Name = $"User {id}",
                Email = $"user{id}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365))
            };
            
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("GET", $"/api/users/{id}", 200, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("user.found", true);
                Activity.Current.SetStatus(ActivityStatusCode.Ok);
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("GET", $"/api/users/{id}", 500, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("error.type", ex.GetType().Name);
                Activity.Current.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            
            _logger.LogError(ex, "Erro ao buscar usuário {UserId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Atualiza usuário
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Endpoint PUT /api/users/{Id} chamado", id);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("user.id", id);
                Activity.Current.SetTag("endpoint.type", "update");
                Activity.Current.SetTag("user.name_updated", !string.IsNullOrEmpty(request.Name));
                Activity.Current.SetTag("user.email_updated", !string.IsNullOrEmpty(request.Email));
            }
            
            var user = await _databaseService.UpdateUserAsync(id, request);
            
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("PUT", $"/api/users/{id}", 200, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetStatus(ActivityStatusCode.Ok);
            }
            
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("PUT", $"/api/users/{id}", 404, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("user.found", false);
                Activity.Current.SetStatus(ActivityStatusCode.Ok, "User not found");
            }
            
            return NotFound(new { error = $"User {id} not found" });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("PUT", $"/api/users/{id}", 500, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("error.type", ex.GetType().Name);
                Activity.Current.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            
            _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly BusinessService _businessService;
    private readonly ILogger<ProcessingController> _logger;

    public ProcessingController(BusinessService businessService, ILogger<ProcessingController> logger)
    {
        _businessService = businessService;
        _logger = logger;
    }

    /// <summary>
    /// Processa lote de dados com telemetria
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<BatchProcessingResult>> ProcessBatch([FromQuery] int size = 100)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Endpoint POST /api/processing/batch chamado com size={Size}", size);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("batch.size", size);
                Activity.Current.SetTag("endpoint.type", "batch_processing");
            }
            
            var result = await _businessService.ProcessDataBatchAsync(size);
            
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("POST", "/api/processing/batch", 200, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("batch.processed_items", result.ProcessedItems);
                Activity.Current.SetTag("batch.error_items", result.ErrorItems);
                Activity.Current.SetTag("batch.success_rate", (double)result.ProcessedItems / result.TotalItems);
                Activity.Current.SetStatus(ActivityStatusCode.Ok);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("POST", "/api/processing/batch", 500, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("error.type", ex.GetType().Name);
                Activity.Current.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            
            _logger.LogError(ex, "Erro no processamento de lote");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Simula operação com múltiplas etapas para demonstrar trace distribuído
    /// </summary>
    [HttpPost("complex")]
    public async Task<ActionResult<object>> ProcessComplexOperation()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Iniciando operação complexa com múltiplas etapas");
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("operation.type", "complex_multi_step");
                Activity.Current.SetTag("endpoint.type", "complex_processing");
            }
            
            var results = new List<object>();
            
            // Etapa 1: Buscar dados
            using (var activity = ApplicationTelemetry.StartActivity("Complex Operation - Step 1: Fetch Data"))
            {
                activity?.SetTag("step", "fetch_data");
                
                await Task.Delay(Random.Shared.Next(100, 300));
                results.Add(new { step = 1, description = "Data fetched", duration_ms = 250 });
                
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            
            // Etapa 2: Processar dados
            using (var activity = ApplicationTelemetry.StartActivity("Complex Operation - Step 2: Process Data"))
            {
                activity?.SetTag("step", "process_data");
                
                await Task.Delay(Random.Shared.Next(200, 500));
                results.Add(new { step = 2, description = "Data processed", duration_ms = 350 });
                
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            
            // Etapa 3: Salvar resultados
            using (var activity = ApplicationTelemetry.StartActivity("Complex Operation - Step 3: Save Results"))
            {
                activity?.SetTag("step", "save_results");
                
                await Task.Delay(Random.Shared.Next(150, 250));
                results.Add(new { step = 3, description = "Results saved", duration_ms = 200 });
                
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("POST", "/api/processing/complex", 200, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("operation.steps_completed", results.Count);
                Activity.Current.SetStatus(ActivityStatusCode.Ok);
            }
            
            var response = new
            {
                success = true,
                total_duration_ms = stopwatch.ElapsedMilliseconds,
                steps = results
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApplicationTelemetry.RecordHttpRequest("POST", "/api/processing/complex", 500, stopwatch.ElapsedMilliseconds);
            
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("error.type", ex.GetType().Name);
                Activity.Current.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            
            _logger.LogError(ex, "Erro na operação complexa");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
