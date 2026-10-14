using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AlzaProducts.Application.Paging;
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
        // Runs on the mock data, whatever data source the application is configured with
        _client = _factory.WithWebHostBuilder(builder => builder.UseSetting("DataSource", "Mock")).CreateClient();
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
        Assert.Equal(SampleNamesInOrder(), products.Select(product => product.Name));
    }

    [Fact]
    public async Task GetPage_WithoutParameters_ReturnsFirstTenProductsOrderedByName()
    {
        var page = await _client.GetFromJsonAsync<PagedResult<ProductDto>>("/api/v2/products");

        Assert.NotNull(page);
        Assert.Equal(SampleNamesInOrder().Take(10), page.Items.Select(product => product.Name));
        Assert.Equal(1, page.Page);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(SampleProducts.Create().Count, page.TotalCount);
    }

    [Fact]
    public async Task GetPage_ReturnsPagingInformationBeforeTheProducts()
    {
        using var json = JsonDocument.Parse(await _client.GetStringAsync("/api/v2/products"));

        string[] expected = ["page", "pageSize", "totalCount", "totalPages", "items"];
        Assert.Equal(expected, json.RootElement.EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public async Task GetPage_AllPagesTogether_EqualTheVersion1List()
    {
        var all = await _client.GetFromJsonAsync<List<ProductDto>>("/api/v1/products");

        var paged = new List<ProductDto>();
        var totalPages = 1;
        for (var number = 1; number <= totalPages; number++)
        {
            var page = await _client.GetFromJsonAsync<PagedResult<ProductDto>>(
                $"/api/v2/products?page={number}&pageSize=7");
            Assert.NotNull(page);
            paged.AddRange(page.Items);
            totalPages = page.TotalPages;
        }

        Assert.Equal(all, paged);
    }

    [Theory]
    [InlineData("page=1000")]
    [InlineData("page=2147483647&pageSize=100")]
    public async Task GetPage_AfterTheLastPage_ReturnsNoProducts(string query)
    {
        var page = await _client.GetFromJsonAsync<PagedResult<ProductDto>>($"/api/v2/products?{query}");

        Assert.NotNull(page);
        Assert.Empty(page.Items);
        Assert.Equal(SampleProducts.Create().Count, page.TotalCount);
    }

    [Theory]
    [InlineData("page=0", "page")]
    [InlineData("page=-1", "page")]
    [InlineData("page=abc", "page")]
    [InlineData("pageSize=0", "pageSize")]
    [InlineData("pageSize=101", "pageSize")]
    public async Task GetPage_WithInvalidPaging_ReturnsValidationProblem(string query, string invalidParameter)
    {
        var response = await _client.GetAsync($"/api/v2/products?{query}");

        var problem = await AssertProblemAsync<ValidationProblemDetails>(response, HttpStatusCode.BadRequest);
        Assert.Contains(invalidParameter, problem.Errors.Keys);
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("v2")]
    public async Task GetById_WithExistingId_ReturnsProduct(string version)
    {
        var expected = SampleProducts.Create()[0];

        var product = await _client.GetFromJsonAsync<ProductDto>($"/api/{version}/products/{expected.Id}");

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

    [Theory]
    [InlineData("v1")]
    [InlineData("v2")]
    public async Task UpdateDescription_StoresAndReturnsNewDescription(string version)
    {
        var id = SampleProducts.Create()[0].Id;

        var response = await _client.PatchAsJsonAsync(
            $"/api/{version}/products/{id}",
            new { description = "New description" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("New description", updated?.Description);
        var stored = await _client.GetFromJsonAsync<ProductDto>($"/api/{version}/products/{id}");
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
    public async Task UpdateDescription_WithUnknownProperty_ReturnsValidationProblemAndKeepsDescription()
    {
        var product = SampleProducts.Create()[0];

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{product.Id}",
            new { description = "New description", price = 1m });

        var problem = await AssertProblemAsync<ValidationProblemDetails>(response, HttpStatusCode.BadRequest);
        Assert.Contains("$.price", problem.Errors.Keys);
        var stored = await _client.GetFromJsonAsync<ProductDto>($"/api/v1/products/{product.Id}");
        Assert.Equal(product.Description, stored?.Description);
    }

    [Fact]
    public async Task UnsupportedApiVersion_ReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync("/api/v3/products");

        await AssertProblemAsync<ProblemDetails>(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiVersionInQueryString_IsIgnored()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/v1/products?api-version=2.0");

        Assert.NotNull(products);
        Assert.Equal(SampleProducts.Create().Count, products.Count);
    }

    [Fact]
    public async Task Responses_ReportTheSupportedApiVersions()
    {
        var response = await _client.GetAsync("/api/v1/products");

        Assert.Equal("1.0, 2.0", string.Join(", ", response.Headers.GetValues("api-supported-versions")));
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("v2")]
    public async Task OpenApiDocument_DescribesAllEndpointsOfItsVersion(string version)
    {
        using var json = JsonDocument.Parse(await _client.GetStringAsync($"/openapi/{version}.json"));

        var paths = json.RootElement.GetProperty("paths");
        string[] expectedPaths = [$"/api/{version}/products", $"/api/{version}/products/{{id}}"];
        Assert.Equal(expectedPaths, paths.EnumerateObject().Select(path => path.Name));
        Assert.True(paths.GetProperty(expectedPaths[0]).TryGetProperty("get", out _));
        Assert.True(paths.GetProperty(expectedPaths[1]).TryGetProperty("get", out _));
        Assert.True(paths.GetProperty(expectedPaths[1]).TryGetProperty("patch", out _));
    }

    [Fact]
    public async Task OpenApiDocument_OfVersion2_DescribesPagingDefaultsAndLimits()
    {
        using var json = JsonDocument.Parse(await _client.GetStringAsync("/openapi/v2.json"));

        var schemas = ParameterSchemas(json.RootElement, "/api/v2/products");
        Assert.Equal(1, schemas["page"].GetProperty("default").GetInt32());
        Assert.Equal(1, schemas["page"].GetProperty("minimum").GetInt32());
        Assert.Equal(10, schemas["pageSize"].GetProperty("default").GetInt32());
        Assert.Equal(PageRequest.MaxPageSize, schemas["pageSize"].GetProperty("maximum").GetInt32());
    }

    // The web JSON defaults also accept numbers written as strings, which the documents would show as "integer | string"
    [Fact]
    public async Task OpenApiDocument_DescribesNumbersAsPlainNumbers()
    {
        using var json = JsonDocument.Parse(await _client.GetStringAsync("/openapi/v2.json"));

        var parameters = ParameterSchemas(json.RootElement, "/api/v2/products");
        var schemas = json.RootElement.GetProperty("components").GetProperty("schemas");
        var pagedResult = schemas.GetProperty("PagedResultOfProductDto").GetProperty("properties");
        var product = schemas.GetProperty("ProductDto").GetProperty("properties");
        Assert.Equal("integer", TypeOf(parameters["page"]));
        Assert.Equal("integer", TypeOf(parameters["pageSize"]));
        Assert.Equal("integer", TypeOf(pagedResult.GetProperty("totalCount")));
        Assert.Equal("number", TypeOf(product.GetProperty("price")));
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("v2")]
    public async Task OpenApiDocument_DescribesTheUpdateBodyAsDescriptionOnly(string version)
    {
        using var json = JsonDocument.Parse(await _client.GetStringAsync($"/openapi/{version}.json"));

        var request = json.RootElement
            .GetProperty("components").GetProperty("schemas").GetProperty("UpdateProductDescriptionRequest");
        string[] expected = ["description"];
        Assert.Equal(expected, request.GetProperty("properties").EnumerateObject().Select(property => property.Name));
        Assert.False(request.GetProperty("additionalProperties").GetBoolean());
    }

    [Fact]
    public async Task SwaggerUi_IsServed()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("swagger-ui", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SwaggerUi_OffersTheDocumentOfEachVersion()
    {
        var script = await _client.GetStringAsync("/swagger/index.js");

        Assert.Contains("/openapi/v1.json", script);
        Assert.Contains("/openapi/v2.json", script);
    }

    private static IEnumerable<string> SampleNamesInOrder() =>
        SampleProducts.Create()
            .Select(product => product.Name)
            .Order(StringComparer.OrdinalIgnoreCase);

    // Schemas of the query parameters of an OpenAPI path's GET operation, by parameter name
    private static Dictionary<string, JsonElement> ParameterSchemas(JsonElement document, string path) =>
        document
            .GetProperty("paths").GetProperty(path).GetProperty("get").GetProperty("parameters")
            .EnumerateArray()
            .ToDictionary(
                parameter => parameter.GetProperty("name").GetString()!,
                parameter => parameter.GetProperty("schema"));

    // A single type comes back as is, a list of types as JSON, e.g. ["integer","string"]
    private static string TypeOf(JsonElement schema) => schema.GetProperty("type").ToString();

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
