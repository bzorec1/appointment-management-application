namespace HairSalonAppointments.Api.Models;

public sealed class WorkingHours
{
    public TimeSpan Start { get; init; }

    public TimeSpan End { get; init; }
}