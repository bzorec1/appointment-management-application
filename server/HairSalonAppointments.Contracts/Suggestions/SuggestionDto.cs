namespace HairSalonAppointments.Contracts.Suggestions;

public sealed class SuggestionDto
{
    public Guid Id { get; set; }
    public DateTime SuggestedStartUtc { get; set; }
    public DateTime SuggestedEndUtc { get; set; }
    public SuggestionState State { get; set; }
    public RequestedBy RequestedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PromotedAtUtc { get; set; }
}
