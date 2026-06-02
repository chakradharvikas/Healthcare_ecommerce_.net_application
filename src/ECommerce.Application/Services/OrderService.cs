using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderDto> CheckoutAsync(string userId, string email, CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var cartItems = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        if (cartItems.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        var order = new Order
        {
            UserId = userId,
            CustomerEmail = email,
            ShippingAddress = dto.ShippingAddress,
            Status = OrderStatus.Confirmed
        };

        foreach (var cartItem in cartItems)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken)
                ?? throw new InvalidOperationException($"Product {cartItem.ProductId} not found.");

            if (product.StockQuantity < cartItem.Quantity)
                throw new InvalidOperationException($"Insufficient stock for {product.Name}.");

            product.StockQuantity -= cartItem.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product, cancellationToken);

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.LineTotal);
        var created = await _orderRepository.AddAsync(order, cancellationToken);
        await _cartRepository.ClearAsync(userId, cancellationToken);

        return MapToDto(created);
    }

    public async Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null || order.UserId != userId) return null;
        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order) =>
        new(
            order.Id,
            order.CustomerEmail,
            order.ShippingAddress,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.LineTotal)).ToList());
}
