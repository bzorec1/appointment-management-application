namespace HairSalonAppointments.Api.Models;

public sealed class Appointment
{
    public Guid Id { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }
}