using System.Text.Json.Serialization;
using Asp.Versioning;

namespace AlzaProducts.Api;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the controllers, standard error responses (ProblemDetails), health checks, API versioning
    /// and one OpenAPI document per API version
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        // Numbers are plain JSON numbers: the web defaults also read "5" as 5, which the OpenAPI documents then
        // describe as "integer | string". Controllers read JSON with the MVC options, the documents use the HTTP ones
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict);
        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
        services.AddProblemDetails();
        services.AddHealthChecks();
        services.AddApiVersioning(options =>
            {
                // The version is a part of the URL (/api/v1/...) and responses list the supported versions in a header
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ReportApiVersions = true;
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                // Documents are named v1, v2, ... and show /api/v1/... instead of /api/v{version}/...
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi();
        return services;
    }
}
