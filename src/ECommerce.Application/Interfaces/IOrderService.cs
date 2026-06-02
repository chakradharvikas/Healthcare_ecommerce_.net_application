namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<DTOs.OrderDto> CheckoutAsync(string userId, string email, DTOs.CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DTOs.OrderDto>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default);
    Task<DTOs.OrderDto?> GetByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default);
}
