namespace HairSalonAppointments.Contracts.Suggestions.Requests;

public sealed record CreateSuggestionRequest
{
    public DateTime? TargetDate { get; init; }

    public TimePreference TimePreference { get; init; }

    public RequestedBy RequestedBy { get; init; }

    public string? ServiceId { get; init; }

    public TimeOnly? PreferredTime { get; init; }
}