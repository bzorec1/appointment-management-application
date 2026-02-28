using Microsoft.Extensions.Logging;

namespace HairSalonAppointments.Api.Infrastructure;

internal static class CorsExtensions
{
    public const string DefaultPolicyName = "Default";

    public static IServiceCollection AddCorsFromConfig(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(opts =>
        {
            opts.AddPolicy(DefaultPolicyName, p =>
            {
                if (origins.Length > 0)
                {
                    p.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders("ETag", "Cache-Control", "Content-Type");
                }
                else
                {
                    var env = config["ASPNETCORE_ENVIRONMENT"] ?? "Production";
                    if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
                    {
                        p.SetIsOriginAllowed(_ => true)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithExposedHeaders("ETag", "Cache-Control", "Content-Type");
                    }
                    else
                    {
                        p.WithOrigins("http://localhost:5500")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithExposedHeaders("ETag", "Cache-Control", "Content-Type");
                    }
                }
            });
        });

        return services;
    }
}