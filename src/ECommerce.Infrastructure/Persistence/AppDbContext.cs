using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Category).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(c => new { c.UserId, c.ProductId }).IsUnique();
            entity.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId);
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.ShippingAddress).HasMaxLength(500).IsRequired();
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });
    }
}
