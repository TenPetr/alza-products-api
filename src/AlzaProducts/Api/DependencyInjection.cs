using Asp.Versioning;

namespace AlzaProducts.Api;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the controllers, standard error responses (ProblemDetails), API versioning
    /// and one OpenAPI document per API version
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
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
