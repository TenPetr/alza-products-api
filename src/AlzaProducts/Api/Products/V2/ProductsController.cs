using System.ComponentModel.DataAnnotations;
using AlzaProducts.Application.Paging;
using AlzaProducts.Application.Products;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AlzaProducts.Api.Products.V2;

/// <summary>
/// Products of the e-shop
/// </summary>
[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>Lists one page of products, ordered by name</summary>
    /// <param name="page">Number of the page, starting at 1</param>
    /// <param name="pageSize">Maximum number of products on the page</param>
    /// <response code="200">The page of products; a page after the last one has no products</response>
    /// <response code="400">The page number or the page size is out of range</response>
    [HttpGet]
    [ProducesResponseType<PagedResult<ProductDto>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPage(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, PageRequest.MaxPageSize)] int pageSize = PageRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var products = await productService.GetPageAsync(new PageRequest(page, pageSize), cancellationToken);
        return Ok(products);
    }

    /// <summary>Gets a product by its id</summary>
    /// <param name="id">Unique identifier of the product</param>
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
    /// <param name="id">Unique identifier of the product</param>
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
