using HairSalonAppointments.Contracts.Calendar;

namespace HairSalonAppointments.Abstractions.Calendar;

public interface ICalendarProvider
{
    public string Key { get; }

    public Task<string> CreateAsync(
        CalendarEventDto @event,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<CalendarEventDto>> ListAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}