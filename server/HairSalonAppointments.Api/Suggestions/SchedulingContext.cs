using HairSalonAppointments.Api.Models;
using HairSalonAppointments.Api.Suggestions.Models;

namespace HairSalonAppointments.Api.Suggestions;

public sealed class SchedulingContext
{
    public IReadOnlyCollection<Appointment> ExistingAppointments { get; init; }
        = Array.Empty<Appointment>();

    public IReadOnlyCollection<WorkingHours> WorkingHours { get; init; }
        = Array.Empty<WorkingHours>();

    public IReadOnlyCollection<TimeSlot> Breaks { get; init; }
        = Array.Empty<TimeSlot>();

    public TimeSpan ServiceDuration { get; init; }
}