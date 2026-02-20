using Dica80.CleanArchitecture.Domain.Common;

namespace Dica80.CleanArchitecture.Domain.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public class Email : ValueObject
{
    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(email.ToLowerInvariant());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;
}

/// <summary>
/// Money value object for handling currency
/// </summary>
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        return new Money(Amount - other.Amount, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:C} {Currency}";
}

/// <summary>
/// Priority value object with predefined levels
/// </summary>
public class Priority : ValueObject
{
    public static readonly Priority Low = new(1, "Low");
    public static readonly Priority Medium = new(2, "Medium");
    public static readonly Priority High = new(3, "High");
    public static readonly Priority Critical = new(4, "Critical");

    public int Level { get; private set; }
    public string Name { get; private set; }

    private Priority(int level, string name)
    {
        Level = level;
        Name = name;
    }

    public static Priority Create(int level)
    {
        return level switch
        {
            1 => Low,
            2 => Medium,
            3 => High,
            4 => Critical,
            _ => throw new ArgumentException("Invalid priority level", nameof(level))
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Level;
    }

    public static implicit operator int(Priority priority) => priority.Level;
    public override string ToString() => Name;
}
