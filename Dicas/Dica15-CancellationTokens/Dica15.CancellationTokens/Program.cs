/*
 * Dica 15: Cancellation Tokens em APIs ASP.NET Core
 * 
 * O CancellationToken permite cancelar operações assíncronas quando o cliente
 * desconecta ou cancela a requisição. Em ASP.NET Core, o framework fornece
 * automaticamente um token para cada requisição.
 * 
 * Regras importantes:
 * - NUNCA crie seu próprio CancellationToken em APIs
 * - Sempre adicione CancellationToken como parâmetro do endpoint
 * - Propague o token para todas as operações assíncronas
 * - Isso permite economizar recursos quando clientes cancelam
 */

using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IDataService, DataService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Demonstração da Dica 15: Cancellation Tokens em APIs ASP.NET Core
Console.WriteLine("=== Dica 15: Cancellation Tokens em APIs ASP.NET Core ===");
Console.WriteLine("Em APIs ASP.NET Core, não crie seus próprios CancellationToken.");
Console.WriteLine("Adicione um parâmetro CancellationToken ao seu endpoint de API.");
Console.WriteLine("O framework ASP.NET Core fornecerá um token específico para sua requisição.");
Console.WriteLine("Se o usuário cancelar a requisição, qualquer processo em cascata também será cancelado.");
Console.WriteLine();

// Endpoint INCORRETO - sem CancellationToken
app.MapGet("/users-incorrect/{id:int}", async (int id, IUserService userService) =>
{
    // ❌ INCORRETO: Não usa CancellationToken da requisição
    var user = await userService.GetUserByIdAsync(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
})
.WithName("GetUserIncorrect")
.WithSummary("Exemplo INCORRETO - sem CancellationToken");

// Endpoint CORRETO - com CancellationToken
app.MapGet("/users/{id:int}", async (int id, IUserService userService, CancellationToken cancellationToken) =>
{
    // ✅ CORRETO: Usa CancellationToken fornecido pelo framework
    var user = await userService.GetUserByIdAsync(id, cancellationToken);
    return user is not null ? Results.Ok(user) : Results.NotFound();
})
.WithName("GetUser")
.WithSummary("Exemplo CORRETO - com CancellationToken");

// Endpoint para operação longa com cascata de cancelamento
app.MapPost("/users/{id:int}/send-email", async (int id, IUserService userService, IEmailService emailService, CancellationToken cancellationToken) =>
{
    try
    {
        // ✅ Passa o CancellationToken para todas as operações subsequentes
        var user = await userService.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
            return Results.NotFound();

        await emailService.SendWelcomeEmailAsync(user.Email, cancellationToken);
        
        return Results.Ok(new { Message = "Email enviado com sucesso", User = user.Name });
    }
    catch (OperationCanceledException)
    {
        return Results.Json(new { Message = "Operação cancelada pelo cliente" }, statusCode: 499);
    }
})
.WithName("SendWelcomeEmail")
.WithSummary("Demonstra cascata de cancelamento");

// Endpoint para processamento de lote com cancelamento
app.MapPost("/users/batch-process", async (IDataService dataService, CancellationToken cancellationToken) =>
{
    try
    {
        var results = await dataService.ProcessLargeBatchAsync(cancellationToken);
        return Results.Ok(new { ProcessedCount = results.Count, Items = results });
    }
    catch (OperationCanceledException)
    {
        return Results.Json(new { Message = "Processamento em lote cancelado" }, statusCode: 499);
    }
})
.WithName("BatchProcess")
.WithSummary("Processamento em lote com suporte a cancelamento");

// Endpoint para demonstrar timeout personalizado
app.MapGet("/users/{id:int}/detailed", async (int id, IUserService userService, IDataService dataService, CancellationToken requestToken) =>
{
    try
    {
        // Combina token da requisição com timeout personalizado
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(requestToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30)); // Timeout de 30 segundos
        
        var user = await userService.GetUserByIdAsync(id, timeoutCts.Token);
        if (user is null)
            return Results.NotFound();

        var detailedData = await dataService.GetDetailedUserDataAsync(id, timeoutCts.Token);
        
        return Results.Ok(new { User = user, Details = detailedData });
    }
    catch (OperationCanceledException)
    {
        return Results.Json(new { Message = "Operação cancelada ou timeout atingido" }, statusCode: 408);
    }
})
.WithName("GetDetailedUser")
.WithSummary("Demonstra combinação de tokens de cancelamento");

app.Run();

// Models
public record User(int Id, string Name, string Email);

// Services que demonstram propagação de CancellationToken
public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private static readonly User[] Users = 
    [
        new(1, "Alice Silva", "alice@email.com"),
        new(2, "Bob Santos", "bob@email.com"),
        new(3, "Carol Lima", "carol@email.com")
    ];

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Simula operação de banco de dados que pode ser cancelada
        await Task.Delay(1000, cancellationToken); // Simula latência
        
        return Users.FirstOrDefault(u => u.Id == id);
    }
}

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    public async Task SendWelcomeEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Iniciando envio de email para: {email}");
        
        // Simula envio de email que pode ser cancelado
        for (int i = 0; i < 10; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(200, cancellationToken);
            Console.WriteLine($"Progresso do email: {(i + 1) * 10}%");
        }
        
        Console.WriteLine($"Email enviado com sucesso para: {email}");
    }
}

public interface IDataService
{
    Task<List<string>> ProcessLargeBatchAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetDetailedUserDataAsync(int userId, CancellationToken cancellationToken = default);
}

public class DataService : IDataService
{
    public async Task<List<string>> ProcessLargeBatchAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<string>();
        
        for (int i = 1; i <= 100; i++)
        {
            // Verifica cancelamento a cada iteração
            cancellationToken.ThrowIfCancellationRequested();
            
            // Simula processamento
            await Task.Delay(50, cancellationToken);
            results.Add($"Item processado: {i}");
            
            if (i % 10 == 0)
                Console.WriteLine($"Processados {i}/100 itens");
        }
        
        return results;
    }

    public async Task<Dictionary<string, object>> GetDetailedUserDataAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Simula múltiplas operações que respeitam cancelamento
        var tasks = new[]
        {
            GetUserProfileAsync(userId, cancellationToken),
            GetUserPreferencesAsync(userId, cancellationToken),
            GetUserActivityAsync(userId, cancellationToken)
        };

        var results = await Task.WhenAll(tasks);
        
        return new Dictionary<string, object>
        {
            ["Profile"] = results[0],
            ["Preferences"] = results[1],
            ["Activity"] = results[2]
        };
    }

    private async Task<object> GetUserProfileAsync(int userId, CancellationToken cancellationToken)
    {
        await Task.Delay(800, cancellationToken);
        return new { UserId = userId, Bio = "Usuário ativo desde 2023", Avatar = "avatar.jpg" };
    }

    private async Task<object> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken)
    {
        await Task.Delay(600, cancellationToken);
        return new { Theme = "Dark", Language = "pt-BR", Notifications = true };
    }

    private async Task<object> GetUserActivityAsync(int userId, CancellationToken cancellationToken)
    {
        await Task.Delay(1200, cancellationToken);
        return new { LastLogin = DateTime.Now.AddHours(-2), LoginCount = 152, LastAction = "Updated profile" };
    }
}
