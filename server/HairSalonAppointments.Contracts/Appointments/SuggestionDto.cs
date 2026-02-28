namespace HairSalonAppointments.Contracts.Appointments;

public readonly record struct SuggestionDto(DateTimeOffset Start, DateTimeOffset End);