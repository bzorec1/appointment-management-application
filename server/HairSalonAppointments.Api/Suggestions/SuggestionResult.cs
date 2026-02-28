using HairSalonAppointments.Api.Suggestions.Models;

namespace HairSalonAppointments.Api.Suggestions;

public sealed class SuggestionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<TimeSlot> Slots { get; init; } = Array.Empty<TimeSlot>();

    public static SuggestionResult Fail(string errorMessage)
        => new() { Success = false, ErrorMessage = errorMessage };

    public static SuggestionResult Ok(IReadOnlyList<TimeSlot> slots)
        => new() { Success = true, Slots = slots };
}
