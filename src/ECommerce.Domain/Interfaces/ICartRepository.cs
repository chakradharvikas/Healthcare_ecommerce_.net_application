using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface ICartRepository
{
    Task<IReadOnlyList<CartItem>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartItem?> GetItemAsync(string userId, Guid productId, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(CartItem item, CancellationToken cancellationToken = default);
    Task RemoveAsync(string userId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearAsync(string userId, CancellationToken cancellationToken = default);
}
