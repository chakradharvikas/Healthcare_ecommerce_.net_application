using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<CartItemDto>> GetCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var items = await _cartRepository.GetByUserIdAsync(userId, cancellationToken);
        return items.Select(i => new CartItemDto(
            i.ProductId,
            i.Product.Name,
            i.Product.Price,
            i.Quantity,
            i.Product.Price * i.Quantity)).ToList();
    }

    public async Task AddToCartAsync(string userId, AddToCartDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId, cancellationToken)
            ?? throw new InvalidOperationException("Product not found.");

        if (!product.IsActive)
            throw new InvalidOperationException("Product is not available.");

        if (product.StockQuantity < dto.Quantity)
            throw new InvalidOperationException("Insufficient stock.");

        var existing = await _cartRepository.GetItemAsync(userId, dto.ProductId, cancellationToken);
        var quantity = existing is null ? dto.Quantity : existing.Quantity + dto.Quantity;

        if (quantity > product.StockQuantity)
            throw new InvalidOperationException("Insufficient stock for requested quantity.");

        await _cartRepository.AddOrUpdateAsync(new CartItem
        {
            UserId = userId,
            ProductId = dto.ProductId,
            Quantity = quantity
        }, cancellationToken);
    }

    public async Task UpdateCartItemAsync(string userId, Guid productId, UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Quantity <= 0)
        {
            await _cartRepository.RemoveAsync(userId, productId, cancellationToken);
            return;
        }

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException("Product not found.");

        if (dto.Quantity > product.StockQuantity)
            throw new InvalidOperationException("Insufficient stock.");

        await _cartRepository.AddOrUpdateAsync(new CartItem
        {
            UserId = userId,
            ProductId = productId,
            Quantity = dto.Quantity
        }, cancellationToken);
    }

    public Task RemoveFromCartAsync(string userId, Guid productId, CancellationToken cancellationToken = default) =>
        _cartRepository.RemoveAsync(userId, productId, cancellationToken);

    public Task ClearCartAsync(string userId, CancellationToken cancellationToken = default) =>
        _cartRepository.ClearAsync(userId, cancellationToken);
}
