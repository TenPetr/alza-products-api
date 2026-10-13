using AlzaProducts.Domain;
using AlzaProducts.Infrastructure.Products;

namespace AlzaProducts.Tests.Infrastructure.Products;

public class InMemoryProductRepositoryTests
{
    private readonly InMemoryProductRepository _repository = new(SampleProducts.Create());

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        var products = await _repository.GetAllAsync();

        Assert.Equal(
            SampleProducts.Create().Select(product => product.Id).Order(),
            products.Select(product => product.Id).Order());
    }

    [Fact]
    public async Task GetAllAsync_OrdersByNameIgnoringCase()
    {
        var repository = new InMemoryProductRepository(
            [CreateProduct("Mouse"), CreateProduct("keyboard"), CreateProduct("Laptop")]);

        var products = await repository.GetAllAsync();

        string[] expected = ["keyboard", "Laptop", "Mouse"];
        Assert.Equal(expected, products.Select(product => product.Name));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
    {
        var expected = SampleProducts.Create()[0];

        var product = await _repository.GetByIdAsync(expected.Id);

        Assert.NotNull(product);
        Assert.Equal(expected.Name, product.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        var product = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(product);
    }

    [Fact]
    public async Task UpdateAsync_StoresChanges()
    {
        var product = (await _repository.GetAllAsync())[0];
        product.UpdateDescription("Updated description");

        await _repository.UpdateAsync(product);

        var stored = await _repository.GetByIdAsync(product.Id);
        Assert.Equal("Updated description", stored?.Description);
    }

    [Fact]
    public async Task ChangesToReturnedProduct_AreNotStoredWithoutUpdate()
    {
        var product = (await _repository.GetAllAsync())[0];
        var originalDescription = product.Description;

        product.UpdateDescription("Changed but not saved");

        var stored = await _repository.GetByIdAsync(product.Id);
        Assert.Equal(originalDescription, stored?.Description);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownProduct_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(CreateProduct("Unknown")));
    }

    [Fact]
    public async Task GetAllAsync_WithCancelledToken_Throws()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => _repository.GetAllAsync(cancellation.Token));
    }

    private static Product CreateProduct(string name) =>
        new(Guid.NewGuid(), name, new Uri("https://cdn.example.com/products/test.jpg"), 100m);
}
