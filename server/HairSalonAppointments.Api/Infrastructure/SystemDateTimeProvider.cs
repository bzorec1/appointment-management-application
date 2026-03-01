using HairSalonAppointments.Abstractions;

namespace HairSalonAppointments.Api.Infrastructure;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;

    public DateTime Today => DateTime.Today;
}