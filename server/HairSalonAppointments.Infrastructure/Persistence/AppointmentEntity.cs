using HairSalonAppointments.Contracts.Appointments;

namespace HairSalonAppointments.Infrastructure.Persistence;

/// <summary>
/// Database entity representing an appointment record.
/// </summary>
public class AppointmentEntity
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required DateTime Start { get; set; }

    public required DateTime End { get; set; }

    public required int ResourceId { get; set; }

    public string? ResourceName { get; set; }

    public required string Phone { get; set; }

    public required string Service { get; set; }

    public string? CustomerName { get; set; }

    public required DateTime CreatedAt { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Confirmed;

    public byte[]? RowVersion { get; set; }

    // Multi-phase service support: active phase is when stylist is working
    public DateTime? ActiveStart { get; set; }
    public DateTime? ActiveEnd { get; set; }

    // Passive phase is when customer waits (e.g., dye processing) and stylist is free
    public DateTime? PassiveStart { get; set; }
    public DateTime? PassiveEnd { get; set; }

    // If this appointment was nested during another appointment's passive phase
    public int? ParentAppointmentId { get; set; }
}