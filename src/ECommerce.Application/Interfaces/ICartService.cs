namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<IReadOnlyList<DTOs.CartItemDto>> GetCartAsync(string userId, CancellationToken cancellationToken = default);
    Task AddToCartAsync(string userId, DTOs.AddToCartDto dto, CancellationToken cancellationToken = default);
    Task UpdateCartItemAsync(string userId, Guid productId, DTOs.UpdateCartItemDto dto, CancellationToken cancellationToken = default);
    Task RemoveFromCartAsync(string userId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(string userId, CancellationToken cancellationToken = default);
}
