using System.Collections.Concurrent;
using AlzaProducts.Application.Products;
using AlzaProducts.Domain;

namespace AlzaProducts.Infrastructure.Products;

/// <summary>
/// Mock data source that keeps products in memory
/// It hands out copies, so like with a database, a change is stored only when UpdateAsync is called
/// </summary>
public sealed class InMemoryProductRepository(IEnumerable<Product> products) : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products =
        new(products.Select(product => KeyValuePair.Create(product.Id, Copy(product))));

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Product> result = _products.Values
            .OrderBy(product => product.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(product => product.Id)
            .Select(Copy)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var product = _products.TryGetValue(id, out var stored) ? Copy(stored) : null;
        return Task.FromResult(product);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_products.ContainsKey(product.Id))
        {
            throw new InvalidOperationException($"Product {product.Id} does not exist");
        }

        _products[product.Id] = Copy(product);
        return Task.CompletedTask;
    }

    private static Product Copy(Product product) =>
        new(product.Id, product.Name, product.ImgUri, product.Price, product.Description);
}
