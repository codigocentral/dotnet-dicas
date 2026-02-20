using System.ComponentModel.DataAnnotations;

namespace Dica50.FluentValidation.Models;

// Modelo básico para demonstrar validações simples
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public string? Phone { get; set; }
    public Address? Address { get; set; }
    public List<string> Interests { get; set; } = new();
    public UserType UserType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Website { get; set; }
    public decimal? Salary { get; set; }
}

// Modelo aninhado para validações complexas
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

// Enum para demonstrar validação de enums
public enum UserType
{
    Regular = 1,
    Premium = 2,
    Admin = 3,
    Guest = 4
}

// Modelo para demonstrar validações condicionais
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsDigital { get; set; }
    public decimal? Weight { get; set; } // Obrigatório apenas para produtos físicos
    public List<string> Tags { get; set; } = new();
    public DateTime? ExpirationDate { get; set; }
    public ProductAvailability Availability { get; set; }
    public string? ManufacturerCode { get; set; }
    public List<ProductReview> Reviews { get; set; } = new();
}

public class ProductReview
{
    public int Id { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerEmail { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
    public bool IsVerified { get; set; }
}

public enum ProductAvailability
{
    InStock = 1,
    OutOfStock = 2,
    PreOrder = 3,
    Discontinued = 4
}

// Modelo para validações assíncronas
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty; // CNPJ no Brasil
    public string Email { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal AnnualRevenue { get; set; }
    public DateTime FoundedDate { get; set; }
    public List<string> Certifications { get; set; } = new();
    public Address HeadquartersAddress { get; set; } = new();
    public string? Website { get; set; }
    public CompanySize Size { get; set; }
}

public enum CompanySize
{
    Startup = 1,
    Small = 2,
    Medium = 3,
    Large = 4,
    Enterprise = 5
}

// DTOs para APIs
public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Phone { get; set; }
    public CreateAddressRequest? Address { get; set; }
    public UserType UserType { get; set; }
    public List<string> Interests { get; set; } = new();
}

public class CreateAddressRequest
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int? Age { get; set; }
    public string? Phone { get; set; }
    public UpdateAddressRequest? Address { get; set; }
    public List<string>? Interests { get; set; }
}

public class UpdateAddressRequest
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
}

// Modelo para demonstrar validações complexas de regras de negócio
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal ShippingCost { get; set; }
    public decimal Discount { get; set; }
    public string? CouponCode { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShippingDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public Address ShippingAddress { get; set; } = new();
    public string? SpecialInstructions { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}

public enum PaymentMethod
{
    CreditCard = 1,
    DebitCard = 2,
    PayPal = 3,
    BankTransfer = 4,
    Cash = 5,
    Pix = 6
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Refunded = 7
}
