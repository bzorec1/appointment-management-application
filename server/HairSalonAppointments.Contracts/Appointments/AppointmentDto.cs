namespace HairSalonAppointments.Contracts.Appointments;

public record AppointmentDto(
    int Id,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    int ResourceId,
    string? ResourceName,
    string Phone,
    string Service,
    string CustomerName,
    DateTimeOffset CreatedAt,
    AppointmentStatus Status = AppointmentStatus.Confirmed,
    DateTimeOffset? ActiveStart = null,
    DateTimeOffset? ActiveEnd = null,
    DateTimeOffset? PassiveStart = null,
    DateTimeOffset? PassiveEnd = null,
    string? CustomerGoogleCalendarUrl = null,
    string? CustomerIcsUrl = null,
    string? SmsPreview = null
);