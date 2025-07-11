using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VendingMachine.Core.Entities;

namespace VendingMachine.Infrastructure.Data;

public class VendingMachineDbContext : IdentityDbContext<User>
{
    public VendingMachineDbContext(DbContextOptions<VendingMachineDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Product entity
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AmountAvailable).IsRequired();
            entity.Property(e => e.Cost).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.SellerId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Seller)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Transaction entity
        builder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BuyerId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.TotalSpent).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.Change).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Buyer)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure User entity
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.Deposit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }
} 