namespace Dica67_APIVersioning.Models;

public record Product
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

// V2 com campos adicionais
public record ProductV2 : Product
{
    public string Category { get; init; } = string.Empty;
    public string[] Tags { get; init; } = Array.Empty<string>();
    public ProductRating? Rating { get; init; }
    public ProductInventory? Inventory { get; init; }
}

// V3 com mudanças significativas
public record ProductV3
{
    public string Sku { get; init; } = string.Empty; // Mudança: ID agora é SKU
    public string Title { get; init; } = string.Empty; // Mudança: Name agora é Title
    public Money Price { get; init; } = new();
    public string Summary { get; init; } = string.Empty; // Mudança: Description agora é Summary
    public ProductMetadata Metadata { get; init; } = new();
    public ProductAvailability Availability { get; init; } = new();
}

public record ProductRating
{
    public double Average { get; init; }
    public int Count { get; init; }
}

public record ProductInventory
{
    public int Stock { get; init; }
    public int Reserved { get; init; }
    public int Available => Stock - Reserved;
}

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
}

public record ProductMetadata
{
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

public record ProductAvailability
{
    public bool InStock { get; init; }
    public int Quantity { get; init; }
    public DateTime? RestockDate { get; init; }
}

// DTOs para diferentes versões
public record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
}

public record CreateProductV2Request : CreateProductRequest
{
    public string Category { get; init; } = string.Empty;
    public string[] Tags { get; init; } = Array.Empty<string>();
}

public record CreateProductV3Request
{
    public string Title { get; init; } = string.Empty;
    public Money Price { get; init; } = new();
    public string Summary { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int InitialStock { get; init; }
}

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public string ApiVersion { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }
}
