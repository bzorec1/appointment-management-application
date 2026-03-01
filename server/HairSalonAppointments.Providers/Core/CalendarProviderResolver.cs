using HairSalonAppointments.Abstractions.Calendar;

namespace HairSalonAppointments.Providers.Core;

public sealed class CalendarProviderResolver(
    IEnumerable<ICalendarProvider> providers) : ICalendarProviderResolver
{
    private readonly Dictionary<string, ICalendarProvider> _byKey =
        providers.ToDictionary(p => p.Key, StringComparer.OrdinalIgnoreCase);

    public ICalendarProvider Get(string key)
    {
        return !_byKey.TryGetValue(key, out var provider)
            ? throw new KeyNotFoundException($"Unknown provider '{key}'.")
            : provider;
    }
}