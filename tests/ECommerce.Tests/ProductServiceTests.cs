using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Moq;

namespace ECommerce.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsActiveProducts()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Test", Price = 10, IsActive = true }
        };

        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(products);

        var service = new ProductService(repo.Object);
        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Test", result[0].Name);
    }

    [Fact]
    public async Task CreateAsync_AddsProduct()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        var service = new ProductService(repo.Object);
        var result = await service.CreateAsync(new("Widget", "A widget", 19.99m, 5, "Home", null));

        Assert.Equal("Widget", result.Name);
        Assert.Equal(19.99m, result.Price);
    }
}

public class OrderServiceTests
{
    [Fact]
    public async Task CheckoutAsync_ThrowsWhenCartEmpty()
    {
        var orderRepo = new Mock<IOrderRepository>();
        var cartRepo = new Mock<ICartRepository>();
        var productRepo = new Mock<IProductRepository>();

        cartRepo.Setup(c => c.GetByUserIdAsync("user1", default)).ReturnsAsync(new List<CartItem>());

        var service = new OrderService(orderRepo.Object, cartRepo.Object, productRepo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CheckoutAsync("user1", "test@test.com", new("123 Main St")));
    }
}
