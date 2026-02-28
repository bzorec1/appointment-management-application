namespace HairSalonAppointments.Contracts.Appointments;

public readonly record struct SuggestionRequest(
    int ResourceId,
    int DurationMin,
    DateTimeOffset Earliest,
    DateTimeOffset Latest,
    int MaxResults = 3
);