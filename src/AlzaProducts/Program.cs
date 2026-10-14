using AlzaProducts.Api;
using AlzaProducts.Application;
using AlzaProducts.Infrastructure;
using AlzaProducts.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Unhandled exceptions and empty error responses (e.g. 404 for an unknown route) are returned as ProblemDetails JSON
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateDatabaseAsync();

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

// Liveness: the process answers. It checks no dependencies, so a database outage doesn't get the app restarted.
// Readiness: every registered check passes (e.g. the database answers), so the app can receive traffic
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.MapControllers();

app.Run();
