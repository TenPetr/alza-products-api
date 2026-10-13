namespace AlzaProducts.Application.Products;

/// <summary>
/// Product use cases offered to the API layer
/// </summary>
public interface IProductService
{
    /// <summary>Returns all products ordered by name, then by id</summary>
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the product with the given id, or null if it does not exist</summary>
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the description of a product, returns the updated product, or null if it does not exist
    /// </summary>
    Task<ProductDto?> UpdateDescriptionAsync(Guid id, string? description, CancellationToken cancellationToken = default);
}
