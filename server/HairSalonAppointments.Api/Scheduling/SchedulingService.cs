using HairSalonAppointments.Abstractions.Appointments;
using HairSalonAppointments.Abstractions.Calendar;
using HairSalonAppointments.Api.Services;
using HairSalonAppointments.Contracts.Appointments;
using HairSalonAppointments.Contracts.Calendar;
using HairSalonAppointments.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace HairSalonAppointments.Api.Scheduling;

public sealed class SchedulingService(
    IAppointmentDataStore appointmentDataStore,
    ICalendarProviderResolver calendarResolver,
    IServiceCatalog serviceCatalog,
    ApplicationDbContext dbContext,
    ILogger<SchedulingService> logger) : ISchedulingService
{
    private const string GoogleProviderKey = "google";
    private readonly IAppointmentDataStore _appointmentDataStore = appointmentDataStore;

    public async Task<AppointmentDto> CreateAsync(NewAppointment appointment,
        CancellationToken cancellationToken = default)
    {
        ValidateAppointment(appointment);

        var enriched = EnrichWithPhases(appointment);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (await _appointmentDataStore.OverlapsAsync(enriched.ResourceId, enriched.Start, enriched.End,
                cancellationToken))
        {
            throw new InvalidOperationException("Overlap for resource.");
        }

        var saved = await _appointmentDataStore.InsertAsync(enriched, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        await SyncToCalendarAsync(saved, cancellationToken);

        return saved;
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, NewAppointment appointment,
        CancellationToken cancellationToken = default)
    {
        ValidateAppointment(appointment);

        var enriched = EnrichWithPhases(appointment);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (await _appointmentDataStore.OverlapsAsync(enriched.ResourceId, enriched.Start, enriched.End,
                excludeId: id, cancellationToken))
        {
            throw new InvalidOperationException("Overlap for resource.");
        }

        var updated = await _appointmentDataStore.UpdateAsync(id, enriched, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        if (updated != null)
            await SyncToCalendarAsync(updated, cancellationToken);

        return updated;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _appointmentDataStore.DeleteAsync(id, cancellationToken);
    }

    private static void ValidateAppointment(NewAppointment appointment)
    {
        if (string.IsNullOrWhiteSpace(appointment.Title))
            throw new ArgumentException("Title is required.", nameof(appointment.Title));

        if (appointment.End <= appointment.Start)
            throw new ArgumentException("End must be after Start.", nameof(appointment.End));

        if (appointment.ResourceId <= 0)
            throw new ArgumentException("ResourceId must be positive.", nameof(appointment.ResourceId));
    }

    private NewAppointment EnrichWithPhases(NewAppointment appointment)
    {
        if (string.IsNullOrEmpty(appointment.ServiceId))
            return appointment with { ActiveStart = appointment.Start, ActiveEnd = appointment.End };

        var service = serviceCatalog.GetService(appointment.ServiceId);
        if (service == null)
            return appointment with { ActiveStart = appointment.Start, ActiveEnd = appointment.End };

        var activeEnd = appointment.Start + service.ActiveDuration;
        if (service.PassiveDuration > TimeSpan.Zero)
        {
            var passiveStart = activeEnd;
            var passiveEnd = passiveStart + service.PassiveDuration;
            return appointment with
            {
                End = passiveEnd,
                ActiveStart = appointment.Start,
                ActiveEnd = activeEnd,
                PassiveStart = passiveStart,
                PassiveEnd = passiveEnd
            };
        }

        return appointment with { ActiveStart = appointment.Start, ActiveEnd = activeEnd };
    }

    private static string? StylistColorId(int resourceId) => resourceId switch
    {
        1 => "7",
        2 => "4",
        _ => null
    };

    private async Task SyncToCalendarAsync(AppointmentDto saved, CancellationToken cancellationToken)
    {
        try
        {
            var calendarEvent = new CalendarEventDto(
                ProviderId: null,
                Title: saved.Title,
                Start: saved.Start,
                End: saved.End,
                TimeZone: "Europe/Ljubljana",
                Notes: $"Stranka: {saved.CustomerName}, Tel: {saved.Phone}, Storitev: {saved.Service}",
                ColorId: StylistColorId(saved.ResourceId)
            );

            var provider = calendarResolver.Get(GoogleProviderKey);
            await provider.CreateAsync(calendarEvent, cancellationToken);

            logger.LogInformation("Calendar event created for appointment {Id}", saved.Id);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Calendar sync failed for appointment {Id}. Appointment was saved locally.", saved.Id);
        }
    }
}
