namespace Dica101;

/// <summary>
/// ❌ INCORRETO: Classe tradicional com muito boilerplate
/// </summary>
public class TraditionalPerson
{
    public string FirstName { get; }
    public string LastName { get; }
    public int Age { get; }
    public string Email { get; }

    public TraditionalPerson(string firstName, string lastName, int age, string email)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Age = age;
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    // Implementação manual de IEquatable
    public override bool Equals(object? obj)
    {
        if (obj is not TraditionalPerson other) return false;
        return FirstName == other.FirstName &&
               LastName == other.LastName &&
               Age == other.Age &&
               Email == other.Email;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, LastName, Age, Email);
    }

    public override string ToString()
    {
        return $"TraditionalPerson {{ FirstName = {FirstName}, LastName = {LastName}, Age = {Age}, Email = {Email} }}";
    }

    // Para criar uma cópia modificada, precisa de método manual
    public TraditionalPerson WithAge(int newAge)
    {
        return new TraditionalPerson(FirstName, LastName, newAge, Email);
    }
}

/// <summary>
/// ✅ CORRETO: Record Class - muito mais conciso e funcional
/// </summary>
public record Person(string FirstName, string LastName, int Age, string Email)
{
    // Propriedades calculadas
    public string FullName => $"{FirstName} {LastName}";
    public bool IsAdult => Age >= 18;
    
    // Métodos adicionais específicos
    public Person CelebrateBirthday() => this with { Age = Age + 1 };
    public Person ChangeEmail(string newEmail) => this with { Email = newEmail };
}

/// <summary>
/// Record Struct para dados pequenos e alta performance
/// </summary>
public record struct Point(double X, double Y)
{
    public double DistanceFromOrigin => Math.Sqrt(X * X + Y * Y);
    public Point MoveTo(double newX, double newY) => new(newX, newY);
    public Point MoveBy(double deltaX, double deltaY) => new(X + deltaX, Y + deltaY);
}

/// <summary>
/// Readonly Record Struct - completamente imutável
/// </summary>
public readonly record struct Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} to {other.Currency}");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);
    
    public string ToString(string format) => $"{Amount.ToString(format)} {Currency}";
}

/// <summary>
/// Record com herança - Address base
/// </summary>
public abstract record Address(string Street, string City, string Country);

/// <summary>
/// Record derivado - endereço residencial
/// </summary>
public record ResidentialAddress(string Street, string City, string Country, string PostalCode) 
    : Address(Street, City, Country)
{
    public bool IsValidPostalCode => !string.IsNullOrWhiteSpace(PostalCode) && PostalCode.Length >= 5;
}

/// <summary>
/// Record derivado - endereço comercial
/// </summary>
public record BusinessAddress(string Street, string City, string Country, string CompanyName, string Department) 
    : Address(Street, City, Country)
{
    public string FullBusinessName => string.IsNullOrWhiteSpace(Department) 
        ? CompanyName 
        : $"{CompanyName} - {Department}";
}
