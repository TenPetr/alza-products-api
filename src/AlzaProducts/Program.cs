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
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
        options.DocumentTitle = "Alza Products API";
    });
}

app.MapControllers();

app.Run();
