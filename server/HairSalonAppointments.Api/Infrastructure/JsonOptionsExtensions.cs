using System.Text.Json.Serialization;

namespace HairSalonAppointments.Api.Infrastructure;

internal static class JsonOptionsExtensions
{
    public static IServiceCollection AddHttpJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.SerializerOptions.PropertyNameCaseInsensitive = true;
            o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        return services;
    }
}