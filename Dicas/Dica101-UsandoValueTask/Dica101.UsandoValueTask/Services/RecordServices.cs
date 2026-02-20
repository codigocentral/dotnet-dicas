namespace Dica101;

/// <summary>
/// Serviço que demonstra o uso prático de Records em cenários reais
/// </summary>
public interface IPersonService
{
    Person CreatePerson(string firstName, string lastName, int age, string email);
    Person UpdatePersonAge(Person person, int newAge);
    Person UpdatePersonEmail(Person person, string newEmail);
    bool ArePersonsEqual(Person person1, Person person2);
    string GetPersonInfo(Person person);
    IEnumerable<Person> FilterAdults(IEnumerable<Person> persons);
}

/// <summary>
/// Implementação que mostra as vantagens dos Records
/// </summary>
public class PersonService : IPersonService
{
    private readonly ILogger<PersonService> _logger;

    public PersonService(ILogger<PersonService> logger)
    {
        _logger = logger;
    }

    public Person CreatePerson(string firstName, string lastName, int age, string email)
    {
        _logger.LogInformation("Criando pessoa: {FirstName} {LastName}", firstName, lastName);
        
        // Record construction é simples e clara
        return new Person(firstName, lastName, age, email);
    }

    public Person UpdatePersonAge(Person person, int newAge)
    {
        _logger.LogInformation("Atualizando idade de {FullName} para {NewAge}", person.FullName, newAge);
        
        // Usando expressão 'with' para imutabilidade
        return person with { Age = newAge };
    }

    public Person UpdatePersonEmail(Person person, string newEmail)
    {
        _logger.LogInformation("Atualizando email de {FullName} para {NewEmail}", person.FullName, newEmail);
        
        // Método específico do record que usa 'with' internamente
        return person.ChangeEmail(newEmail);
    }

    public bool ArePersonsEqual(Person person1, Person person2)
    {
        _logger.LogInformation("Comparando pessoas: {Person1} e {Person2}", person1, person2);
        
        // Comparação por valor automática
        return person1 == person2;
    }

    public string GetPersonInfo(Person person)
    {
        // ToString() automático dos records
        return person.ToString();
    }

    public IEnumerable<Person> FilterAdults(IEnumerable<Person> persons)
    {
        _logger.LogInformation("Filtrando pessoas adultas");
        
        // Usando propriedade calculada do record
        return persons.Where(p => p.IsAdult);
    }
}

/// <summary>
/// Serviço para operações com coordenadas usando Record Structs
/// </summary>
public interface IGeometryService
{
    Point CreatePoint(double x, double y);
    Point MovePoint(Point point, double deltaX, double deltaY);
    double CalculateDistance(Point point1, Point point2);
    Point[] CreatePolygon(params (double x, double y)[] coordinates);
}

/// <summary>
/// Implementação que demonstra Record Structs para performance
/// </summary>
public class GeometryService : IGeometryService
{
    private readonly ILogger<GeometryService> _logger;

    public GeometryService(ILogger<GeometryService> logger)
    {
        _logger = logger;
    }

    public Point CreatePoint(double x, double y)
    {
        _logger.LogInformation("Criando ponto em ({X}, {Y})", x, y);
        return new Point(x, y);
    }

    public Point MovePoint(Point point, double deltaX, double deltaY)
    {
        _logger.LogInformation("Movendo ponto {Point} por ({DeltaX}, {DeltaY})", point, deltaX, deltaY);
        
        // Usando método do record struct
        return point.MoveBy(deltaX, deltaY);
    }

    public double CalculateDistance(Point point1, Point point2)
    {
        var distance = Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        _logger.LogInformation("Distância entre {Point1} e {Point2}: {Distance:F2}", point1, point2, distance);
        return distance;
    }

    public Point[] CreatePolygon(params (double x, double y)[] coordinates)
    {
        _logger.LogInformation("Criando polígono com {Count} pontos", coordinates.Length);
        
        // Conversão de tuplas para record structs
        return coordinates.Select(coord => new Point(coord.x, coord.y)).ToArray();
    }
}

/// <summary>
/// Serviço financeiro que demonstra Readonly Record Structs
/// </summary>
public interface IFinancialService
{
    Money CreateMoney(decimal amount, string currency);
    Money AddMoney(Money money1, Money money2);
    Money MultiplyMoney(Money money, decimal factor);
    Money ConvertCurrency(Money money, string targetCurrency, decimal exchangeRate);
    string FormatMoney(Money money, string format);
}

/// <summary>
/// Implementação que mostra segurança de tipos com Readonly Record Structs
/// </summary>
public class FinancialService : IFinancialService
{
    private readonly ILogger<FinancialService> _logger;

    public FinancialService(ILogger<FinancialService> logger)
    {
        _logger = logger;
    }

    public Money CreateMoney(decimal amount, string currency)
    {
        _logger.LogInformation("Criando valor monetário: {Amount} {Currency}", amount, currency);
        return new Money(amount, currency);
    }

    public Money AddMoney(Money money1, Money money2)
    {
        _logger.LogInformation("Somando {Money1} + {Money2}", money1, money2);
        
        // O método Add já valida a moeda
        return money1.Add(money2);
    }

    public Money MultiplyMoney(Money money, decimal factor)
    {
        _logger.LogInformation("Multiplicando {Money} por {Factor}", money, factor);
        return money.Multiply(factor);
    }

    public Money ConvertCurrency(Money money, string targetCurrency, decimal exchangeRate)
    {
        _logger.LogInformation("Convertendo {Money} para {TargetCurrency} (taxa: {Rate})", 
            money, targetCurrency, exchangeRate);
        
        var convertedAmount = money.Amount * exchangeRate;
        return new Money(convertedAmount, targetCurrency);
    }

    public string FormatMoney(Money money, string format)
    {
        return money.ToString(format);
    }
}
