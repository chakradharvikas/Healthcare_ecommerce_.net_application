using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<CartItem>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<CartItem?> GetItemAsync(string userId, Guid productId, CancellationToken cancellationToken = default) =>
        await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, cancellationToken);

    public async Task AddOrUpdateAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        var existing = await GetItemAsync(item.UserId, item.ProductId, cancellationToken);
        if (existing is null)
        {
            _context.CartItems.Add(item);
        }
        else
        {
            existing.Quantity = item.Quantity;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.CartItems.Update(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(string userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(userId, productId, cancellationToken);
        if (item is null) return;
        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearAsync(string userId, CancellationToken cancellationToken = default)
    {
        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync(cancellationToken);
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
