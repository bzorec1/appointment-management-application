namespace HairSalonAppointments.Api.Models;

public sealed record Appointment
{
    public Guid Id { get; set; }

    public DateTime StartUtc { get; init; }

    public DateTime EndUtc { get; init; }
}