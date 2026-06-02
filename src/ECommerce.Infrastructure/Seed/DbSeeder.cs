using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Identity;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("Customer"))
            await roleManager.CreateAsync(new IdentityRole("Customer"));

        if (await userManager.FindByEmailAsync("admin@ecommerce.com") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@ecommerce.com",
                Email = "admin@ecommerce.com",
                FullName = "Store Admin",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (!await context.Products.AnyAsync())
        {
            context.Products.AddRange(
                new Product { Name = "Wireless Headphones", Description = "Noise-cancelling over-ear headphones", Price = 149.99m, StockQuantity = 50, Category = "Electronics", ImageUrl = "https://picsum.photos/seed/headphones/400/300" },
                new Product { Name = "Smart Watch", Description = "Fitness tracking smart watch", Price = 199.99m, StockQuantity = 30, Category = "Electronics", ImageUrl = "https://picsum.photos/seed/watch/400/300" },
                new Product { Name = "Running Shoes", Description = "Lightweight running shoes", Price = 89.99m, StockQuantity = 100, Category = "Sports", ImageUrl = "https://picsum.photos/seed/shoes/400/300" },
                new Product { Name = "Yoga Mat", Description = "Non-slip premium yoga mat", Price = 34.99m, StockQuantity = 75, Category = "Sports", ImageUrl = "https://picsum.photos/seed/yoga/400/300" },
                new Product { Name = "Coffee Maker", Description = "Programmable drip coffee maker", Price = 79.99m, StockQuantity = 40, Category = "Home", ImageUrl = "https://picsum.photos/seed/coffee/400/300" },
                new Product { Name = "Desk Lamp", Description = "LED desk lamp with adjustable brightness", Price = 45.99m, StockQuantity = 60, Category = "Home", ImageUrl = "https://picsum.photos/seed/lamp/400/300" }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded sample products.");
        }
    }
}
