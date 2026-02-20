using FluentAssertions;
using Dica80.CleanArchitecture.Domain.Entities;
using Dica80.CleanArchitecture.Domain.ValueObjects;
using Dica80.CleanArchitecture.Domain.Enums;
using Dica80.CleanArchitecture.Domain.Events;

namespace Dica80.CleanArchitecture.Tests.Domain;

/// <summary>
/// Tests for User entity
/// </summary>
public class UserTests
{
    [Fact]
    public void Create_ValidInput_ShouldCreateUser()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var name = "John Doe";
        var role = UserRole.Member;

        // Act
        var user = User.Create(email, name, role);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.Name.Should().Be(name);
        user.Role.Should().Be(role);
        user.IsActive.Should().BeTrue();
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().Should().BeOfType<UserCreatedEvent>();
    }

    [Fact]
    public void Activate_InactiveUser_ShouldActivateUser()
    {
        // Arrange
        var user = CreateTestUser();
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ActiveUser_ShouldDeactivateUserAndRaiseDomainEvent()
    {
        // Arrange
        var user = CreateTestUser();
        user.ClearDomainEvents(); // Clear creation event

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().Should().BeOfType<UserDeactivatedEvent>();
    }

    [Fact]
    public void UpdateInfo_ValidInput_ShouldUpdateUserInfo()
    {
        // Arrange
        var user = CreateTestUser();
        var newName = "Jane Doe";
        var newRole = UserRole.Manager;

        // Act
        user.UpdateInfo(newName, newRole);

        // Assert
        user.Name.Should().Be(newName);
        user.Role.Should().Be(newRole);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateInfo_InvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        var act = () => user.UpdateInfo(invalidName, UserRole.Member);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Name cannot be empty*");
    }

    [Fact]
    public void UpdateLastLogin_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var user = CreateTestUser();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        user.UpdateLastLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeOnOrAfter(beforeUpdate);
    }

    private static User CreateTestUser()
    {
        var email = Email.Create("test@example.com");
        return User.Create(email, "Test User", UserRole.Member);
    }
}

/// <summary>
/// Tests for Email value object
/// </summary>
public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("admin+test@company.org")]
    public void Create_ValidEmail_ShouldCreateEmail(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user name@domain.com")]
    public void Create_InvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var act = () => Email.Create(invalidEmail);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Invalid email format*");
    }

    [Fact]
    public void Create_EmailWithUppercase_ShouldConvertToLowercase()
    {
        // Arrange
        var upperCaseEmail = "Test@EXAMPLE.COM";

        // Act
        var email = Email.Create(upperCaseEmail);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Equals_SameValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }
}

/// <summary>
/// Tests for Money value object
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Create_ValidInput_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "USD";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        money.Should().NotBeNull();
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_NegativeAmount_ShouldThrowArgumentException(decimal negativeAmount)
    {
        // Act & Assert
        var act = () => Money.Create(negativeAmount, "USD");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Amount cannot be negative*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Create_InvalidCurrency_ShouldThrowArgumentException(string invalidCurrency)
    {
        // Act & Assert
        var act = () => Money.Create(100, invalidCurrency);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Currency must be a 3-character code*");
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(50, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var usd = Money.Create(100, "USD");
        var eur = Money.Create(50, "EUR");

        // Act & Assert
        var act = () => usd.Add(eur);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot add money with different currencies*");
    }
}
