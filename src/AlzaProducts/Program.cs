using AlzaProducts.Api;
using AlzaProducts.Application;
using AlzaProducts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

// Unhandled exceptions and empty error responses (e.g. 404 for an unknown route) are returned as ProblemDetails JSON
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // One OpenAPI document and Swagger UI definition per API version (v1, v2, ...)
    app.MapOpenApi().WithDocumentPerVersion();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            options.SwaggerEndpoint($"/openapi/{description.GroupName}.json", description.GroupName);
        }

        options.DocumentTitle = "Alza Products API";
    });
}

app.MapControllers();

app.Run();
