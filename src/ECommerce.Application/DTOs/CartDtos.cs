namespace ECommerce.Application.DTOs;

public record CartItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record AddToCartDto(Guid ProductId, int Quantity);

public record UpdateCartItemDto(int Quantity);
