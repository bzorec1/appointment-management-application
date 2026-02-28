namespace HairSalonAppointments.Abstractions.Calendar;

public interface ICalendarProviderResolver
{
    public ICalendarProvider Get(string key);
}