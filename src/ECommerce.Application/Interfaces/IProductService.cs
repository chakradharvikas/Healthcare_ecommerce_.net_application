namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<DTOs.ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DTOs.ProductDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<DTOs.ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DTOs.ProductDto> CreateAsync(DTOs.CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<DTOs.ProductDto?> UpdateAsync(Guid id, DTOs.UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
