using AlzaProducts.Application.Paging;
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
    public async Task GetPageAsync_ReturnsRequestedPageInTheSameOrderAsTheFullList()
    {
        var all = await _repository.GetAllAsync();

        var page = await _repository.GetPageAsync(new PageRequest(2, 10));

        Assert.Equal(all.Skip(10).Take(10).Select(product => product.Id), page.Items.Select(product => product.Id));
        Assert.Equal(2, page.Page);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(all.Count, page.TotalCount);
    }

    [Fact]
    public async Task GetPageAsync_LastPage_ContainsTheRemainingProducts()
    {
        var repository = new InMemoryProductRepository([CreateProduct("A"), CreateProduct("B"), CreateProduct("C")]);

        var page = await repository.GetPageAsync(new PageRequest(2, 2));

        Assert.Equal("C", Assert.Single(page.Items).Name);
        Assert.Equal(3, page.TotalCount);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public async Task GetPageAsync_AfterTheLastPage_ReturnsNoProducts(int pageNumber)
    {
        var repository = new InMemoryProductRepository([CreateProduct("A"), CreateProduct("B"), CreateProduct("C")]);

        var page = await repository.GetPageAsync(new PageRequest(pageNumber, 2));

        Assert.Empty(page.Items);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task GetPageAsync_WithCancelledToken_Throws()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.GetPageAsync(new PageRequest(), cancellation.Token));
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
