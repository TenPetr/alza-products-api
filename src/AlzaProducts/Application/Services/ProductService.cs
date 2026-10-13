using AlzaProducts.Application.Dtos;
using AlzaProducts.Application.Interfaces;

namespace AlzaProducts.Application.Services;

public sealed class ProductService(IProductRepository repository, ILogger<ProductService> logger) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await repository.GetAllAsync(cancellationToken);
        return products.Select(ProductDto.FromProduct).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ProductDto.FromProduct(product);
    }

    public async Task<ProductDto?> UpdateDescriptionAsync(
        Guid id,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.UpdateDescription(description);
        await repository.UpdateAsync(product, cancellationToken);

        logger.LogInformation("Updated description of product {ProductId}", id);
        return ProductDto.FromProduct(product);
    }
}
