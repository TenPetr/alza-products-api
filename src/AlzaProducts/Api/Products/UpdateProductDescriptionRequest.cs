using System.ComponentModel.DataAnnotations;
using AlzaProducts.Domain;

namespace AlzaProducts.Api.Products;

/// <summary>
/// New description of a product
/// </summary>
public sealed record UpdateProductDescriptionRequest
{
    /// <summary>The new description; null or an empty string removes it</summary>
    [MaxLength(Product.DescriptionMaxLength)]
    public required string? Description { get; init; }
}
