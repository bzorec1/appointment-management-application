using HairSalonAppointments.Contracts.Appointments;

namespace HairSalonAppointments.Abstractions.Appointments;

public interface ISchedulingService
{
    public Task<AppointmentDto> CreateAsync(
        NewAppointment appointment,
        CancellationToken cancellationToken = default);

    public Task<AppointmentDto?> UpdateAsync(
        int id,
        NewAppointment appointment,
        CancellationToken cancellationToken = default);

    public Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
