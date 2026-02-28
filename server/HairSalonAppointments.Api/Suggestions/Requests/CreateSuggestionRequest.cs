using HairSalonAppointments.Api.Suggestions.Enums;
using HairSalonAppointments.Contracts.Suggestions;

namespace HairSalonAppointments.Api.Suggestions.Requests;

public sealed class CreateSuggestionRequest
{
    public DateTime? TargetDate { get; init; }

    public TimePreference TimePreference { get; init; }

    public RequestedBy RequestedBy { get; init; }

    public string? ServiceId { get; init; }

    public TimeOnly? PreferredTime { get; init; }
}