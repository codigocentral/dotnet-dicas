namespace Dica101;

/// <summary>
/// Demonstra as funcionalidades e benefícios dos Record Types
/// </summary>
public class RecordDemonstration
{
    private readonly ILogger<RecordDemonstration> _logger;
    private readonly IPersonService _personService;
    private readonly IGeometryService _geometryService;
    private readonly IFinancialService _financialService;

    public RecordDemonstration(
        ILogger<RecordDemonstration> logger,
        IPersonService personService,
        IGeometryService geometryService,
        IFinancialService financialService)
    {
        _logger = logger;
        _personService = personService;
        _geometryService = geometryService;
        _financialService = financialService;
    }

    /// <summary>
    /// Demonstra comparação por valor automática
    /// </summary>
    public void DemonstrateValueEquality()
    {
        _logger.LogInformation("=== Demonstração: Comparação por Valor ===");

        // Records têm comparação por valor automática
        var person1 = new Person("João", "Silva", 25, "joao@test.com");
        var person2 = new Person("João", "Silva", 25, "joao@test.com");
        var person3 = new Person("Maria", "Santos", 30, "maria@test.com");

        _logger.LogInformation("person1 == person2: {AreEqual}", person1 == person2);
        _logger.LogInformation("person1.Equals(person2): {AreEqual}", person1.Equals(person2));
        _logger.LogInformation("person1 == person3: {AreEqual}", person1 == person3);

        // Classes tradicionais comparam por referência
        var traditional1 = new TraditionalPerson("João", "Silva", 25, "joao@test.com");
        var traditional2 = new TraditionalPerson("João", "Silva", 25, "joao@test.com");

        _logger.LogInformation("\nClasse tradicional (com Equals implementado):");
        _logger.LogInformation("traditional1 == traditional2: {AreEqual}", traditional1 == traditional2); // false (referência)
        _logger.LogInformation("traditional1.Equals(traditional2): {AreEqual}", traditional1.Equals(traditional2)); // true (valor)

        // Record Structs também têm comparação por valor
        var point1 = new Point(1.0, 2.0);
        var point2 = new Point(1.0, 2.0);
        _logger.LogInformation("\nRecord Struct:");
        _logger.LogInformation("point1 == point2: {AreEqual}", point1 == point2);
    }

    /// <summary>
    /// Demonstra expressões 'with' para imutabilidade
    /// </summary>
    public void DemonstrateWithExpressions()
    {
        _logger.LogInformation("\n=== Demonstração: Expressões 'with' ===");

        var originalPerson = new Person("João", "Silva", 25, "joao@test.com");
        _logger.LogInformation("Pessoa original: {Person}", originalPerson);

        // Criar cópias modificadas usando 'with'
        var olderPerson = originalPerson with { Age = 30 };
        var renamedPerson = originalPerson with { FirstName = "João Pedro", Email = "joaopedro@test.com" };
        var celebratedPerson = originalPerson.CelebrateBirthday();

        _logger.LogInformation("Pessoa mais velha: {Person}", olderPerson);
        _logger.LogInformation("Pessoa renomeada: {Person}", renamedPerson);
        _logger.LogInformation("Aniversário celebrado: {Person}", celebratedPerson);

        // Verificar que o original não mudou
        _logger.LogInformation("Original inalterado: {Person}", originalPerson);

        // Expressões with com record structs
        var originalPoint = new Point(1.0, 2.0);
        var movedPoint = originalPoint with { X = 5.0 };
        _logger.LogInformation("\nPoint original: {Point}", originalPoint);
        _logger.LogInformation("Point movido: {Point}", movedPoint);
    }

    /// <summary>
    /// Demonstra desestruturação (deconstruction)
    /// </summary>
    public void DemonstrateDeconstruction()
    {
        _logger.LogInformation("\n=== Demonstração: Desestruturação ===");

        var person = new Person("Maria", "Santos", 28, "maria@test.com");

        // Desestruturação total
        var (firstName, lastName, age, email) = person;
        _logger.LogInformation("Desestruturado: {FirstName} {LastName}, {Age} anos, {Email}", 
            firstName, lastName, age, email);

        // Desestruturação parcial com descarte
        var (first, last, _, _) = person;
        _logger.LogInformation("Apenas nome: {FirstName} {LastName}", first, last);

        // Desestruturação com record struct
        var point = new Point(3.5, 7.2);
        var (x, y) = point;
        _logger.LogInformation("Coordenadas: X={X}, Y={Y}", x, y);

        // Desestruturação em pattern matching
        var description = person switch
        {
            ("Maria", _, var personAge, _) when personAge >= 18 => "Maria é adulta",
            (var name, _, _, _) => $"{name} é menor de idade",
        };
        _logger.LogInformation("Pattern matching: {Description}", description);
    }

    /// <summary>
    /// Demonstra herança de records
    /// </summary>
    public void DemonstrateRecordInheritance()
    {
        _logger.LogInformation("\n=== Demonstração: Herança de Records ===");

        var residential = new ResidentialAddress("Rua das Flores, 123", "São Paulo", "Brasil", "01234-567");
        var business = new BusinessAddress("Av. Paulista, 1000", "São Paulo", "Brasil", "Tech Corp", "TI");

        _logger.LogInformation("Endereço residencial: {Address}", residential);
        _logger.LogInformation("CEP válido: {IsValid}", residential.IsValidPostalCode);

        _logger.LogInformation("Endereço comercial: {Address}", business);
        _logger.LogInformation("Nome completo da empresa: {CompanyName}", business.FullBusinessName);

        // Polimorfismo funciona normalmente
        Address[] addresses = { residential, business };
        foreach (var address in addresses)
        {
            _logger.LogInformation("Endereço genérico: {City}, {Country}", address.City, address.Country);
        }

        // Expressões with funcionam com herança
        var newResidential = residential with { PostalCode = "99999-999" };
        _logger.LogInformation("Novo CEP: {Address}", newResidential);
    }

    /// <summary>
    /// Demonstra record structs para alta performance
    /// </summary>
    public void DemonstrateRecordStructPerformance()
    {
        _logger.LogInformation("\n=== Demonstração: Record Structs para Performance ===");

        // Record struct regular
        var point = new Point(1.0, 2.0);
        _logger.LogInformation("Point inicial: {Point}", point);
        _logger.LogInformation("Distância da origem: {Distance:F2}", point.DistanceFromOrigin);

        // Operações que retornam novos valores
        var movedPoint = point.MoveBy(3.0, 4.0);
        _logger.LogInformation("Point movido: {Point}", movedPoint);

        // Readonly record struct para máxima imutabilidade
        var money1 = new Money(100.50m, "BRL");
        var money2 = new Money(50.25m, "BRL");

        _logger.LogInformation("Dinheiro 1: {Money}", money1);
        _logger.LogInformation("Dinheiro 2: {Money}", money2);

        var total = money1.Add(money2);
        var doubled = money1.Multiply(2);

        _logger.LogInformation("Total: {Money}", total);
        _logger.LogInformation("Dobrado: {Money}", doubled);
        _logger.LogInformation("Formatado: {Money}", money1.ToString("C"));

        // Tentativa de operação inválida
        try
        {
            var usd = new Money(100, "USD");
            var invalid = money1.Add(usd); // Vai gerar exceção
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Operação inválida: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Demonstra serialização JSON com records
    /// </summary>
    public void DemonstrateJsonSerialization()
    {
        _logger.LogInformation("\n=== Demonstração: Serialização JSON ===");

        var person = new Person("Carlos", "Oliveira", 35, "carlos@test.com");
        
        // Serializar para JSON
        var json = JsonSerializer.Serialize(person, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        _logger.LogInformation("Person serializado:\n{Json}", json);

        // Deserializar de JSON
        var deserializedPerson = JsonSerializer.Deserialize<Person>(json);
        _logger.LogInformation("Person deserializado: {Person}", deserializedPerson);

        // Verificar igualdade
        _logger.LogInformation("São iguais: {AreEqual}", person == deserializedPerson);

        // Serialização com record struct
        var point = new Point(10.5, 20.3);
        var pointJson = JsonSerializer.Serialize(point);
        _logger.LogInformation("Point JSON: {Json}", pointJson);

        var deserializedPoint = JsonSerializer.Deserialize<Point>(pointJson);
        _logger.LogInformation("Point deserializado: {Point}", deserializedPoint);
    }

    /// <summary>
    /// Demonstra casos de uso práticos
    /// </summary>
    public void DemonstratePracticalUseCases()
    {
        _logger.LogInformation("\n=== Demonstração: Casos de Uso Práticos ===");

        // 1. DTO para API
        var userDto = new Person("Ana", "Costa", 29, "ana.costa@company.com");
        _logger.LogInformation("DTO de usuário: {User}", userDto);

        // 2. Value Objects para domínio
        var coordinate = new Point(-23.5505, -46.6333); // São Paulo
        _logger.LogInformation("Coordenadas de São Paulo: {Coordinate}", coordinate);

        // 3. Configuração imutável
        var price = new Money(299.99m, "BRL");
        var discountedPrice = price.Multiply(0.9m); // 10% desconto
        _logger.LogInformation("Preço original: {Price}", price);
        _logger.LogInformation("Preço com desconto: {Price}", discountedPrice);

        // 4. Estado de aplicação
        var currentUser = userDto with { Age = userDto.Age + 1 };
        _logger.LogInformation("Usuário após aniversário: {User}", currentUser);

        // 5. Pattern matching avançado
        var classification = userDto switch
        {
            { Age: >= 18 and < 30, FirstName: var name } => $"{name} é jovem adulto",
            { Age: >= 30 and < 60, FirstName: var name } => $"{name} é adulto",
            { Age: >= 60, FirstName: var name } => $"{name} é sênior",
            var user => $"{user.FirstName} é menor de idade"
        };
        _logger.LogInformation("Classificação: {Classification}", classification);
    }
}
