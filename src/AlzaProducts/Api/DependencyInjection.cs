using Microsoft.OpenApi;

namespace AlzaProducts.Api;

public static class DependencyInjection
{
    /// <summary>Registers the controllers, standard error responses (ProblemDetails) and the OpenAPI document</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Alza Products API",
                Version = "v1",
                Description = "Products of an e-shop, with a partial update of a single product",
            };
            return Task.CompletedTask;
        }));
        return services;
    }
}
