using AlzaProducts.Api.Products;
using AlzaProducts.Api.Products.V2;
using AlzaProducts.Application.Paging;
using AlzaProducts.Application.Products;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace AlzaProducts.Tests.Api.Products.V2;

public class ProductsControllerTests
{
    private readonly IProductService _productService = Substitute.For<IProductService>();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_productService);
    }

    [Fact]
    public async Task GetPage_ReturnsOkWithRequestedPage()
    {
        var page = new PagedResult<ProductDto>([CreateProduct()], 2, 5, 6);
        _productService.GetPageAsync(new PageRequest(2, 5), Arg.Any<CancellationToken>()).Returns(page);

        var result = await _controller.GetPage(2, 5, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(page, ok.Value);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsOkWithProduct()
    {
        var product = CreateProduct();
        _productService.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _controller.GetById(product.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(product, ok.Value);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        _productService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProductDto?)null);

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateDescription_WithExistingId_ReturnsOkWithUpdatedProduct()
    {
        var updated = CreateProduct() with { Description = "New description" };
        _productService
            .UpdateDescriptionAsync(updated.Id, "New description", Arg.Any<CancellationToken>())
            .Returns(updated);

        var result = await _controller.UpdateDescription(
            updated.Id,
            new UpdateProductDescriptionRequest { Description = "New description" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updated, ok.Value);
    }

    [Fact]
    public async Task UpdateDescription_WithUnknownId_ReturnsNotFound()
    {
        _productService
            .UpdateDescriptionAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((ProductDto?)null);

        var result = await _controller.UpdateDescription(
            Guid.NewGuid(),
            new UpdateProductDescriptionRequest { Description = "New description" },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private static ProductDto CreateProduct() =>
        new(Guid.NewGuid(), "Laptop", new Uri("https://cdn.example.com/products/laptop.jpg"), 999m, "Fast and light");
}
