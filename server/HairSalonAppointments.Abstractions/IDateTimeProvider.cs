namespace HairSalonAppointments.Abstractions;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime Today { get; }
}
