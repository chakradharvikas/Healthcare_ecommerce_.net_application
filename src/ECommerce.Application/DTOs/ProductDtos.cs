namespace ECommerce.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category,
    string? ImageUrl,
    bool IsActive);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category,
    string? ImageUrl);

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category,
    string? ImageUrl,
    bool IsActive);
