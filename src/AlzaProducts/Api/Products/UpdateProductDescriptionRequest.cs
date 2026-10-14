using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AlzaProducts.Domain;

namespace AlzaProducts.Api.Products;

/// <summary>
/// New description of a product; any other property is rejected
/// </summary>
// Rejecting e.g. "price" with 400 tells the client it cannot be changed here, instead of silently ignoring it
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateProductDescriptionRequest
{
    /// <summary>The new description; null or an empty string removes it</summary>
    [MaxLength(Product.DescriptionMaxLength)]
    public required string? Description { get; init; }
}
