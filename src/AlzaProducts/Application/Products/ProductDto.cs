using AlzaProducts.Domain;

namespace AlzaProducts.Application.Products;

/// <summary>
/// A product as returned to API clients
/// </summary>
public sealed record ProductDto(Guid Id, string Name, Uri ImgUri, decimal Price, string? Description)
{
    public static ProductDto FromProduct(Product product) =>
        new(product.Id, product.Name, product.ImgUri, product.Price, product.Description);
}
