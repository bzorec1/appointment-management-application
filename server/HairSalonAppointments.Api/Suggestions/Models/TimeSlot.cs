namespace HairSalonAppointments.Api.Suggestions.Models;

public sealed class TimeSlot
{
    public DateTime StartUtc { get; init; }

    public DateTime EndUtc { get; init; }
}