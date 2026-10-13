using AlzaProducts.Domain;

namespace AlzaProducts.Application.Products;

/// <summary>
/// A product as returned to API clients
/// </summary>
/// <param name="Id">Unique identifier of the product</param>
/// <param name="Name">Name of the product</param>
/// <param name="ImgUri">Absolute URL of the product image</param>
/// <param name="Price">Price of the product</param>
/// <param name="Description">Optional description of the product</param>
public sealed record ProductDto(Guid Id, string Name, Uri ImgUri, decimal Price, string? Description)
{
    public static ProductDto FromProduct(Product product) =>
        new(product.Id, product.Name, product.ImgUri, product.Price, product.Description);
}
