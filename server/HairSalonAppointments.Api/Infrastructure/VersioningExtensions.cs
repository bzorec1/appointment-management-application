using Asp.Versioning;

namespace HairSalonAppointments.Api.Infrastructure;

internal static class VersioningExtensions
{
    public static IServiceCollection AddApiVersioningAndExplorer(
        this IServiceCollection services)
    {
        services
            .AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
            })
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}