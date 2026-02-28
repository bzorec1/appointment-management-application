using HairSalonAppointments.Contracts.Appointments;

namespace HairSalonAppointments.Abstractions.Appointments;

public interface IAppointmentDataStore
{
    public Task<bool> OverlapsAsync(
        int resourceId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    public Task<bool> OverlapsAsync(
        int resourceId,
        DateTimeOffset start,
        DateTimeOffset end,
        int? excludeId,
        CancellationToken cancellationToken = default);

    public Task<AppointmentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    public Task<AppointmentDto> InsertAsync(
        NewAppointment appointment,
        CancellationToken cancellationToken = default);

    public Task<AppointmentDto?> UpdateAsync(
        int id,
        NewAppointment appointment,
        CancellationToken cancellationToken = default);

    public Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<AppointmentDto>> ListAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}
