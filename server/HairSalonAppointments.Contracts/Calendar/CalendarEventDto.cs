namespace HairSalonAppointments.Contracts.Calendar;

public record CalendarEventDto(
    string? ProviderId,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    string TimeZone,
    string? Notes = null,
    string? ColorId = null
);