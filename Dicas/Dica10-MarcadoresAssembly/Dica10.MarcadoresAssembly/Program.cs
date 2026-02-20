/*
 * Dica 10: Marcadores de Assembly para Injeção de Dependência
 * 
 * Ao registrar serviços automaticamente (MediatR, AutoMapper, FluentValidation),
 * precisamos indicar qual assembly deve ser escaneado. Usar interfaces marcadoras
 * vazias é a prática recomendada em vez de usar Program.cs ou classes aleatórias.
 * 
 * Benefícios:
 * - Código mais limpo e intencional
 * - Facilita refatoração
 * - Auto-documentado
 * - Evita erros ao mover classes entre assemblies
 */

using Dica10.MarcadoresAssembly;
using Dica10.MarcadoresAssembly.Commands;
using Dica10.MarcadoresAssembly.Models;
using FluentValidation;

Console.WriteLine("🎯 Demonstração: Marcadores de Assembly para Injeção de Dependência");
Console.WriteLine("=" + new string('=', 70));
Console.WriteLine();

// Configurar host com DI usando marcadores de assembly
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // ✅ CORRETO: Usando IAssemblyMarker como marcador de assembly
        
        // MediatR - registra todos os handlers e behaviors do assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());
        
        // AutoMapper - registra todos os profiles do assembly
        services.AddAutoMapper(typeof(IAssemblyMarker));
        
        // FluentValidation - registra todos os validadores do assembly
        services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>();
    })
    .Build();

Console.WriteLine("🚀 Sistema configurado usando IAssemblyMarker!");
Console.WriteLine();

// Demonstrar conceitos
Console.WriteLine(new string('-', 70));
AssemblyMarkerDemo.DemonstrarRegistroIncorreto();

Console.WriteLine(new string('-', 70));
AssemblyMarkerDemo.DemonstrarRegistroCorreto();

Console.WriteLine(new string('-', 70));
AssemblyMarkerDemo.DemonstrarExemplosReais();

Console.WriteLine(new string('-', 70));
AssemblyMarkerDemo.DemonstrarPadroesNomeacao();

Console.WriteLine(new string('-', 70));
AssemblyMarkerDemo.DemonstrarMultiplosAssemblies();

Console.WriteLine(new string('-', 70));
AssemblyMarkerDemo.DemonstrarAlternativas();

Console.WriteLine(new string('-', 70));
Console.WriteLine("🧪 Testando integração dos componentes registrados:");
Console.WriteLine();

// Testar MediatR
var mediator = host.Services.GetRequiredService<IMediator>();
Console.WriteLine("📤 Testando MediatR:");
var users = await mediator.Send(new GetUsersQuery());
foreach (var user in users)
{
    Console.WriteLine($"   • {user.Name} ({user.Email}) - {user.Status} - Criado: {user.FormattedCreatedAt}");
}
Console.WriteLine();

// Testar AutoMapper
var mapper = host.Services.GetRequiredService<IMapper>();
Console.WriteLine("🔄 Testando AutoMapper:");
var createDto = new CreateUserDto { Name = "Novo Usuário", Email = "novo@email.com" };
var mappedUser = mapper.Map<User>(createDto);
var displayDto = mapper.Map<UserDisplayDto>(mappedUser);
Console.WriteLine($"   CreateDto → User → DisplayDto: {displayDto.Name} ({displayDto.Email})");
Console.WriteLine();

// Testar FluentValidation
var validator = host.Services.GetRequiredService<IValidator<CreateUserDto>>();
Console.WriteLine("✅ Testando FluentValidation:");

// Teste com dados válidos
var validDto = new CreateUserDto { Name = "João Silva", Email = "joao@email.com" };
var validResult = await validator.ValidateAsync(validDto);
Console.WriteLine($"   Dados válidos: {validResult.IsValid}");

// Teste com dados inválidos
var invalidDto = new CreateUserDto { Name = "", Email = "email-inválido" };
var invalidResult = await validator.ValidateAsync(invalidDto);
Console.WriteLine($"   Dados inválidos: {invalidResult.IsValid}");
if (!invalidResult.IsValid)
{
    foreach (var error in invalidResult.Errors)
    {
        Console.WriteLine($"      • {error.ErrorMessage}");
    }
}
Console.WriteLine();

Console.WriteLine("🎉 Demonstração concluída!");
Console.WriteLine();
Console.WriteLine("💡 Resumo da Dica:");
Console.WriteLine("   • Crie interfaces marcadoras vazias para cada assembly");
Console.WriteLine("   • Use nomes descritivos como IAssemblyMarker, IApplicationMarker");
Console.WriteLine("   • Evite usar Program.cs como marcador de assembly");
Console.WriteLine("   • Melhora legibilidade e manutenibilidade do código");
Console.WriteLine("   • Facilita refatoração sem quebrar registros DI");
Console.WriteLine("   • Torna intenção do código mais clara e auto-documentada");
