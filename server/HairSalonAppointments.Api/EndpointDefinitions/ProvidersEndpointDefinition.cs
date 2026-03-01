using HairSalonAppointments.Abstractions.Calendar;
using HairSalonAppointments.Contracts.Calendar;

namespace HairSalonAppointments.Api.EndpointDefinitions;

public sealed class ProvidersEndpointDefinition : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
    }

    public void DefineEndpoints(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/providers/{key}");

        group.MapPost("/events", async (
                string key,
                CalendarEventDto dto,
                ICalendarProviderResolver resolver,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var provider = resolver.Get(key);
                    var id = await provider.CreateAsync(dto, cancellationToken);
                    return Results.Created($"/api/v1/providers/{key}/events/{id}", new { id });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        message = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("CreateCalendarEvent");

        group.MapGet("/events", async (
                string key,
                DateTimeOffset from,
                DateTimeOffset to,
                ICalendarProviderResolver resolver,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var provider = resolver.Get(key);
                    var events = await provider.ListAsync(from, to, cancellationToken);
                    return Results.Ok(events);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        message = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("ListCalendarEvents");
    }
}