using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AlzaProducts.Application.Products;
using AlzaProducts.Domain;
using AlzaProducts.Infrastructure.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AlzaProducts.Tests.Api.Products;

/// <summary>
/// Calls the endpoints over HTTP, against the whole application running in memory on the mock data
/// </summary>
public sealed class ProductsApiTests : IDisposable
{
    // A new application per test, so a change made by one test never leaks into another
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public ProductsApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetAll_ReturnsAllProductsOrderedByName()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/v1/products");

        Assert.NotNull(products);
        var expectedNames = SampleProducts.Create()
            .Select(product => product.Name)
            .Order(StringComparer.OrdinalIgnoreCase);
        Assert.Equal(expectedNames, products.Select(product => product.Name));
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsProduct()
    {
        var expected = SampleProducts.Create()[0];

        var product = await _client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{expected.Id}");

        Assert.Equal(ProductDto.FromProduct(expected), product);
    }

    [Fact]
    public async Task GetById_ReturnsJsonWithAttributeNamesFromTheSpecification()
    {
        var id = SampleProducts.Create()[0].Id;

        using var json = JsonDocument.Parse(await _client.GetStringAsync($"/api/v1/products/{id}"));

        string[] expected = ["id", "name", "imgUri", "price", "description"];
        Assert.Equal(expected, json.RootElement.EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync($"/api/v1/products/{Guid.NewGuid()}");

        await AssertProblemAsync<ProblemDetails>(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithMalformedId_ReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync("/api/v1/products/not-a-guid");

        await AssertProblemAsync<ProblemDetails>(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDescription_StoresAndReturnsNewDescription()
    {
        var id = SampleProducts.Create()[0].Id;

        var response = await _client.PatchAsJsonAsync($"/api/v1/products/{id}", new { description = "New description" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("New description", updated?.Description);
        var stored = await _client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{id}");
        Assert.Equal("New description", stored?.Description);
    }

    [Fact]
    public async Task UpdateDescription_WithNull_RemovesDescription()
    {
        var id = SampleProducts.Create()[0].Id;

        var response = await _client.PatchAsJsonAsync($"/api/v1/products/{id}", new { description = (string?)null });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(updated);
        Assert.Null(updated.Description);
    }

    [Fact]
    public async Task UpdateDescription_WithUnknownId_ReturnsNotFoundProblem()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{Guid.NewGuid()}",
            new { description = "New description" });

        await AssertProblemAsync<ProblemDetails>(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDescription_WithTooLongDescription_ReturnsValidationProblemAndKeepsDescription()
    {
        var product = SampleProducts.Create()[0];
        var description = new string('a', Product.DescriptionMaxLength + 1);

        var response = await _client.PatchAsJsonAsync($"/api/v1/products/{product.Id}", new { description });

        var problem = await AssertProblemAsync<ValidationProblemDetails>(response, HttpStatusCode.BadRequest);
        Assert.Contains("Description", problem.Errors.Keys);
        var stored = await _client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{product.Id}");
        Assert.Equal(product.Description, stored?.Description);
    }

    [Fact]
    public async Task UpdateDescription_WithoutDescriptionProperty_ReturnsValidationProblem()
    {
        var id = SampleProducts.Create()[0].Id;

        var response = await _client.PatchAsJsonAsync($"/api/v1/products/{id}", new { });

        await AssertProblemAsync<ValidationProblemDetails>(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OpenApiDocument_DescribesAllEndpoints()
    {
        using var json = JsonDocument.Parse(await _client.GetStringAsync("/openapi/v1.json"));

        var paths = json.RootElement.GetProperty("paths");
        Assert.True(paths.GetProperty("/api/v1/products").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/v1/products/{id}").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/v1/products/{id}").TryGetProperty("patch", out _));
    }

    [Fact]
    public async Task SwaggerUi_IsServed()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("swagger-ui", await response.Content.ReadAsStringAsync());
    }

    private static async Task<TProblem> AssertProblemAsync<TProblem>(HttpResponseMessage response, HttpStatusCode status)
        where TProblem : ProblemDetails
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<TProblem>();
        Assert.NotNull(problem);
        Assert.Equal((int)status, problem.Status);
        return problem;
    }
}
