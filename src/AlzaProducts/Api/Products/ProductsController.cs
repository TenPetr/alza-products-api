using AlzaProducts.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace AlzaProducts.Api.Products;

/// <summary>
/// Products of the e-shop
/// </summary>
[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>Lists all products, ordered by name</summary>
    /// <response code="200">All products</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>Gets a product by its id</summary>
    /// <response code="200">The product</response>
    /// <response code="404">No product with this id exists</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Updates the description of a product</summary>
    /// <response code="200">The updated product</response>
    /// <response code="400">The request is invalid, for example the description is too long</response>
    /// <response code="404">No product with this id exists</response>
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ProductDto>> UpdateDescription(
        Guid id,
        UpdateProductDescriptionRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateDescriptionAsync(id, request.Description, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}
