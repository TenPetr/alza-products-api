using AlzaProducts.Domain;

namespace AlzaProducts.Application.Interfaces;

/// <summary>
/// Storage of products
/// </summary>
public interface IProductRepository
{
    /// <summary>Returns all products ordered by name, then by id</summary>
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the product with the given id, or null if it does not exist</summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Persists changes made to an existing product</summary>
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}
