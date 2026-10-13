using AlzaProducts.Application.Products;
using AlzaProducts.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AlzaProducts.Tests.Application.Products;

public class ProductServiceTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_repository, NullLogger<ProductService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProductsAsDtos()
    {
        var laptop = CreateProduct("Laptop", "Fast and light");
        var phone = CreateProduct("Phone");
        IReadOnlyList<Product> products = [laptop, phone];
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(products);

        var result = await _service.GetAllAsync();

        ProductDto[] expected =
        [
            new(laptop.Id, "Laptop", laptop.ImgUri, laptop.Price, "Fast and light"),
            new(phone.Id, "Phone", phone.ImgUri, phone.Price, null),
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetAllAsync_WithNoProducts_ReturnsEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Product>());

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_PassesCancellationTokenToRepository()
    {
        using var cancellation = new CancellationTokenSource();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Product>());

        await _service.GetAllAsync(cancellation.Token);

        await _repository.Received(1).GetAllAsync(cancellation.Token);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
    {
        var product = CreateProduct("Laptop");
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _service.GetByIdAsync(product.Id);

        Assert.Equal(new ProductDto(product.Id, "Laptop", product.ImgUri, product.Price, null), result);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateDescriptionAsync_WithExistingId_UpdatesSavesAndReturnsProduct()
    {
        var product = CreateProduct("Laptop", "Old description");
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _service.UpdateDescriptionAsync(product.Id, "New description");

        Assert.NotNull(result);
        Assert.Equal("New description", result.Description);
        Assert.Equal("New description", product.Description);
        await _repository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDescriptionAsync_WithUnknownId_ReturnsNullAndDoesNotSave()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _service.UpdateDescriptionAsync(Guid.NewGuid(), "New description");

        Assert.Null(result);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDescriptionAsync_WithTooLongDescription_ThrowsAndDoesNotSave()
    {
        var product = CreateProduct("Laptop", "Old description");
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var description = new string('a', Product.DescriptionMaxLength + 1);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.UpdateDescriptionAsync(product.Id, description));

        Assert.Equal("Old description", product.Description);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    private static Product CreateProduct(string name, string? description = null) =>
        new(
            Guid.NewGuid(),
            name,
            new Uri($"https://cdn.example.com/products/{name.ToLowerInvariant()}.jpg"),
            999m,
            description);
}
