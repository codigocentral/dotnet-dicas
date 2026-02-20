using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Dica44.MediatR.Commands;
using Dica44.MediatR.Queries;
using Dica44.MediatR.Behaviors;
using Dica44.MediatR.Notifications;
using Dica44.MediatR.Models;
using System.Reflection;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 DICA 44: MediatR - Padrão Mediator com CQRS");
        Console.WriteLine("=" + new string('=', 60));

        // Configurar Host com Dependency Injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Registrar repositório
                services.AddSingleton<UserRepository>();

                // Registrar MediatR
                services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                });

                // Validators
                services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

                // Logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            })
            .Build();

        var mediator = host.Services.GetRequiredService<IMediator>();

        try
        {
            // Demonstrar Commands
            Console.WriteLine("\n🔸 COMMANDS (Write Operations)");
            var user1 = await mediator.Send(new CreateUserCommand("João Silva", "joao@email.com"));
            Console.WriteLine($"✅ Usuário criado: {user1.Name}");

            var user2 = await mediator.Send(new CreateUserCommand("Maria Santos", "maria@email.com"));
            Console.WriteLine($"✅ Usuário criado: {user2.Name}");

            // Demonstrar Queries
            Console.WriteLine("\n🔸 QUERIES (Read Operations)");
            var allUsers = await mediator.Send(new GetAllUsersQuery());
            Console.WriteLine($"📋 Total de usuários: {allUsers.Count()}");

            var foundUser = await mediator.Send(new GetUserByIdQuery(user1.Id));
            Console.WriteLine($"🔍 Usuário encontrado: {foundUser?.Name}");

            // Demonstrar Notifications
            Console.WriteLine("\n🔸 NOTIFICATIONS (Pub/Sub)");
            await mediator.Publish(new UserCreatedNotification(user1));
            Console.WriteLine("📢 Notification publicada");

            // Demonstrar Pipeline Behaviors
            Console.WriteLine("\n🔸 PIPELINE BEHAVIORS");
            var slowResult = await mediator.Send(new SlowOperationCommand("Demo", 500));
            Console.WriteLine($"⚡ Operação concluída: {slowResult.Message}");

            Console.WriteLine("\n🎉 DEMONSTRAÇÃO CONCLUÍDA!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }

        Console.WriteLine("\n🎉 DEMONSTRAÇÃO CONCLUÍDA!");
        Console.WriteLine("Pressione Ctrl+C para sair ou aguarde 3 segundos...");
        await Task.Delay(3000);
    }
}
