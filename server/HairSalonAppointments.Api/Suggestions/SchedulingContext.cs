using HairSalonAppointments.Api.Models;
using HairSalonAppointments.Api.Suggestions.Models;

namespace HairSalonAppointments.Api.Suggestions;

public sealed class SchedulingContext
{
    public IReadOnlyCollection<Appointment> ExistingAppointments { get; init; }
        = [];

    public IReadOnlyCollection<WorkingHours> WorkingHours { get; init; }
        = [];

    public IReadOnlyCollection<TimeSlot> Breaks { get; init; }
        = [];

    public TimeSpan ServiceDuration { get; init; }
}