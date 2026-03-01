namespace HairSalonAppointments.Api.Models;

public sealed record WorkingHours
{
    public TimeSpan Start { get; init; }

    public TimeSpan End { get; init; }
}