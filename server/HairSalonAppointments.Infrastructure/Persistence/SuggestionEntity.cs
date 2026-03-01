using HairSalonAppointments.Contracts.Suggestions;

namespace HairSalonAppointments.Infrastructure.Persistence;

public sealed class SuggestionEntity
{
    public Guid Id { get; set; }

    public DateTime SuggestedStartUtc { get; set; }

    public DateTime SuggestedEndUtc { get; set; }

    public SuggestionState State { get; set; }

    public RequestedBy RequestedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? PromotedAtUtc { get; set; }
}