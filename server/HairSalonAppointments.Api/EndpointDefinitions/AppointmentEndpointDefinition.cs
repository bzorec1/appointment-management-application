using System.Text;
using HairSalonAppointments.Abstractions.Appointments;
using HairSalonAppointments.Contracts.Appointments;

namespace HairSalonAppointments.Api.EndpointDefinitions;

public sealed class AppointmentEndpointDefinition : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services) { }

    public void DefineEndpoints(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/appointments");

        group.MapPost("/", async (NewAppointment dto, ISchedulingService sched, CancellationToken ct) =>
            {
                try
                {
                    var saved = await sched.CreateAsync(dto, ct);
                    return Results.Created($"/api/v1/appointments/{saved.Id}", saved);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (InvalidOperationException)
                {
                    return Results.Conflict(new { message = "Termin se prekriva za izbranega stilista." });
                }
            })
            .WithName("CreateAppointment");

        group.MapGet("/", async (DateTimeOffset from, DateTimeOffset to, IAppointmentDataStore dataStore, CancellationToken ct) =>
            {
                if (to <= from) return Results.BadRequest(new { message = "'to' must be after 'from'." });
                var list = await dataStore.ListAsync(from, to, ct);
                return Results.Ok(list);
            })
            .WithName("ListAppointments");

        group.MapGet("/{id:int}", async (int id, IAppointmentDataStore dataStore, CancellationToken ct) =>
            {
                var appointment = await dataStore.GetByIdAsync(id, ct);
                return appointment is not null ? Results.Ok(appointment) : Results.NotFound();
            })
            .WithName("GetAppointment");

        group.MapPut("/{id:int}", async (int id, NewAppointment dto, ISchedulingService sched, CancellationToken ct) =>
            {
                try
                {
                    var updated = await sched.UpdateAsync(id, dto, ct);
                    return updated is not null ? Results.Ok(updated) : Results.NotFound();
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (InvalidOperationException)
                {
                    return Results.Conflict(new { message = "Termin se prekriva za izbranega stilista." });
                }
            })
            .WithName("UpdateAppointment");

        group.MapDelete("/{id:int}", async (int id, ISchedulingService sched, CancellationToken ct) =>
            {
                var deleted = await sched.DeleteAsync(id, ct);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteAppointment");

        group.MapGet("/{id:int}/calendar.ics", async (int id, IAppointmentDataStore dataStore, CancellationToken ct) =>
            {
                var appt = await dataStore.GetByIdAsync(id, ct);
                if (appt is null) return Results.NotFound();
                var ics = BuildIcs(appt);
                return Results.File(Encoding.UTF8.GetBytes(ics), "text/calendar", $"appointment-{id}.ics");
            })
            .WithName("GetAppointmentIcs");
    }

    private static string BuildIcs(AppointmentDto a)
    {
        var dtStamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        var dtStart = a.Start.UtcDateTime.ToString("yyyyMMddTHHmmssZ");
        var dtEnd = a.End.UtcDateTime.ToString("yyyyMMddTHHmmssZ");
        var description = $"Storitev: {a.Service}\\nStranka: {a.CustomerName}\\nTel: {a.Phone}";

        return $"""
BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//HairSalon//Appointments//EN
BEGIN:VEVENT
UID:{a.Id}@hairsalon
DTSTAMP:{dtStamp}
DTSTART:{dtStart}
DTEND:{dtEnd}
SUMMARY:{a.Title}
DESCRIPTION:{description}
END:VEVENT
END:VCALENDAR
""".ReplaceLineEndings("\r\n");
    }
}
