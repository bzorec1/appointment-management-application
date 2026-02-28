using System.Globalization;
using HairSalonAppointments.Abstractions.Appointments;
using HairSalonAppointments.Contracts.Appointments;
using Microsoft.EntityFrameworkCore;

namespace HairSalonAppointments.Infrastructure.Persistence;

public class AppointmentDataDataStore(ApplicationDbContext context) : IAppointmentDataStore
{
    private readonly ApplicationDbContext _context = context;

    public async Task<bool> OverlapsAsync(
        int resourceId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        return await OverlapsAsync(resourceId, start, end, excludeId: null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> OverlapsAsync(
        int resourceId,
        DateTimeOffset start,
        DateTimeOffset end,
        int? excludeId,
        CancellationToken cancellationToken = default)
    {
        var startUtc = start.UtcDateTime;
        var endUtc = end.UtcDateTime;
        var query = _context.Appointments
            .Where(a => a.ResourceId == resourceId && a.Start < endUtc && a.End > startUtc);

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<AppointmentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<AppointmentDto>> ListAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = from.UtcDateTime;
        var toUtc = to.UtcDateTime;
        var list = await _context.Appointments
            .Where(a => a.Start < toUtc && a.End > fromUtc)
            .OrderBy(a => a.Start)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. list.Select(MapToDto)];
    }

    public async Task<AppointmentDto> InsertAsync(
        NewAppointment appointment,
        CancellationToken cancellationToken = default)
    {
        var entity = new AppointmentEntity
        {
            Title = appointment.Title,
            Start = appointment.Start.UtcDateTime,
            End = appointment.End.UtcDateTime,
            ResourceId = appointment.ResourceId,
            ResourceName = null,
            Phone = appointment.Phone,
            Service = appointment.Service,
            CustomerName = appointment.CustomerName,
            CreatedAt = DateTime.UtcNow,
            ActiveStart = appointment.ActiveStart?.UtcDateTime,
            ActiveEnd = appointment.ActiveEnd?.UtcDateTime,
            PassiveStart = appointment.PassiveStart?.UtcDateTime,
            PassiveEnd = appointment.PassiveEnd?.UtcDateTime
        };

        _context.Appointments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapToDto(entity);
    }

    public async Task<AppointmentDto?> UpdateAsync(
        int id,
        NewAppointment appointment,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
            return null;

        entity.Title = appointment.Title;
        entity.Start = appointment.Start.UtcDateTime;
        entity.End = appointment.End.UtcDateTime;
        entity.ResourceId = appointment.ResourceId;
        entity.Phone = appointment.Phone;
        entity.Service = appointment.Service;
        entity.CustomerName = appointment.CustomerName;
        entity.ActiveStart = appointment.ActiveStart?.UtcDateTime;
        entity.ActiveEnd = appointment.ActiveEnd?.UtcDateTime;
        entity.PassiveStart = appointment.PassiveStart?.UtcDateTime;
        entity.PassiveEnd = appointment.PassiveEnd?.UtcDateTime;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
            return false;

        _context.Appointments.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }

    private static AppointmentDto MapToDto(AppointmentEntity a)
    {
        var start = new DateTimeOffset(a.Start, TimeSpan.Zero);
        var end = new DateTimeOffset(a.End, TimeSpan.Zero);

        var gcalDates = $"{a.Start:yyyyMMddTHHmmssZ}/{a.End:yyyyMMddTHHmmssZ}";
        var gcalTitle = Uri.EscapeDataString(a.Title);
        var gcalDetails = Uri.EscapeDataString(
            $"Storitev: {a.Service}\nStranka: {a.CustomerName}\nTel: {a.Phone}");
        var googleCalendarUrl =
            $"https://calendar.google.com/calendar/render?action=TEMPLATE&text={gcalTitle}&dates={gcalDates}&details={gcalDetails}";

        var icsUrl = $"/api/v1/appointments/{a.Id}/calendar.ics";

        string? smsPreview = null;
        if (!string.IsNullOrWhiteSpace(a.Phone))
        {
            var dateStr = a.Start.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            var timeStr = a.Start.ToString("HH:mm", CultureInfo.InvariantCulture);
            smsPreview = $"Spoštovani {a.CustomerName}, vaš termin ({a.Service}) je potrjen za {dateStr} ob {timeStr}. Frizerski salon.";
        }

        return new AppointmentDto(
            a.Id,
            a.Title,
            start,
            end,
            a.ResourceId,
            a.ResourceName,
            a.Phone,
            a.Service,
            a.CustomerName!,
            new DateTimeOffset(a.CreatedAt, TimeSpan.Zero),
            a.Status,
            a.ActiveStart.HasValue ? new DateTimeOffset(a.ActiveStart.Value, TimeSpan.Zero) : null,
            a.ActiveEnd.HasValue ? new DateTimeOffset(a.ActiveEnd.Value, TimeSpan.Zero) : null,
            a.PassiveStart.HasValue ? new DateTimeOffset(a.PassiveStart.Value, TimeSpan.Zero) : null,
            a.PassiveEnd.HasValue ? new DateTimeOffset(a.PassiveEnd.Value, TimeSpan.Zero) : null,
            googleCalendarUrl,
            icsUrl,
            smsPreview
        );
    }
}
