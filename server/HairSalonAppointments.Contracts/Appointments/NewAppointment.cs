namespace HairSalonAppointments.Contracts.Appointments;

public sealed record NewAppointment(
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    int ResourceId,
    string Phone,
    string Service,
    string CustomerName,
    string? ServiceId = null,
    DateTimeOffset? ActiveStart = null,
    DateTimeOffset? ActiveEnd = null,
    DateTimeOffset? PassiveStart = null,
    DateTimeOffset? PassiveEnd = null
);