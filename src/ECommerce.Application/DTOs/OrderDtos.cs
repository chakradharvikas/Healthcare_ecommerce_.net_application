using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record OrderDto(
    Guid Id,
    string CustomerEmail,
    string ShippingAddress,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items);

public record CreateOrderDto(string ShippingAddress);

public record RegisterDto(string Email, string Password, string FullName);

public record LoginDto(string Email, string Password);

public record AuthResponseDto(string Token, string Email, string UserId, string FullName);
