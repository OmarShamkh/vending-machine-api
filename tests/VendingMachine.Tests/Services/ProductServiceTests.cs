using System;
using System.Threading.Tasks;
using FluentAssertions;
using VendingMachine.Core.DTOs;
using VendingMachine.Core.Entities;
using VendingMachine.Infrastructure.Data;
using VendingMachine.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ProductServiceTests
{
    private readonly DbContextOptions<VendingMachineDbContext> _dbOptions;

    public ProductServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<VendingMachineDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProduct()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var seller = new User { Id = 1, Username = "seller", Role = VendingMachine.Core.Enums.UserRole.Seller, PasswordHash = "hash" };
        context.Users.Add(seller);
        context.SaveChanges();
        var service = new ProductService(context);
        var dto = new CreateProductDto { ProductName = "Cola", AmountAvailable = 10, Cost = 50 };
        var result = await service.CreateAsync(dto, seller.Id);
        result.ProductName.Should().Be("Cola");
        result.AmountAvailable.Should().Be(10);
        result.Cost.Should().Be(50);
        result.SellerId.Should().Be(seller.Id);
        result.RowVersion.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var seller = new User { Id = 1, Username = "seller", Role = VendingMachine.Core.Enums.UserRole.Seller, PasswordHash = "hash" };
        var product = new Product { ProductName = "Cola", AmountAvailable = 10, Cost = 50, SellerId = 1, Seller = seller };
        context.Users.Add(seller);
        context.Products.Add(product);
        context.SaveChanges();
        var service = new ProductService(context);
        var dto = new UpdateProductDto { ProductName = "Pepsi", RowVersion = product.RowVersion };
        var result = await service.UpdateAsync(product.Id, dto, seller.Id);
        result.ProductName.Should().Be("Pepsi");
        result.RowVersion.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ConcurrencyConflict_Throws()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var seller = new User { Id = 1, Username = "seller", Role = VendingMachine.Core.Enums.UserRole.Seller, PasswordHash = "hash" };
        var product = new Product { ProductName = "Cola", AmountAvailable = 10, Cost = 50, SellerId = 1, Seller = seller };
        context.Users.Add(seller);
        context.Products.Add(product);
        context.SaveChanges();
        var service = new ProductService(context);
        var wrongRowVersion = new byte[] { 1, 2, 3 };
        var dto = new UpdateProductDto { ProductName = "Pepsi", RowVersion = wrongRowVersion };
        Func<Task> act = async () => await service.UpdateAsync(product.Id, dto, seller.Id);
        await act.Should().ThrowAsync<Exception>().WithMessage("Product update failed due to a concurrent update*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var seller = new User { Id = 1, Username = "seller", Role = VendingMachine.Core.Enums.UserRole.Seller, PasswordHash = "hash" };
        var product = new Product { ProductName = "Cola", AmountAvailable = 10, Cost = 50, SellerId = 1, Seller = seller };
        context.Users.Add(seller);
        context.Products.Add(product);
        context.SaveChanges();
        var service = new ProductService(context);
        var result = await service.DeleteAsync(product.Id, seller.Id);
        result.Should().BeTrue();
        (await context.Products.FindAsync(product.Id)).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnProducts()
    {
        using var context = new VendingMachineDbContext(_dbOptions);
        var seller = new User { Id = 1, Username = "seller", Role = VendingMachine.Core.Enums.UserRole.Seller, PasswordHash = "hash" };
        var product = new Product { ProductName = "Cola", AmountAvailable = 10, Cost = 50, SellerId = 1, Seller = seller };
        context.Users.Add(seller);
        context.Products.Add(product);
        context.SaveChanges();
        var service = new ProductService(context);
        var result = await service.GetAllAsync();
        result.Should().HaveCount(1);
        result[0].ProductName.Should().Be("Cola");
    }
} 