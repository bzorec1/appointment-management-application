using System.Globalization;
using HairSalonAppointments.Abstractions.Calendar;
using HairSalonAppointments.Contracts.Calendar;
using HairSalonAppointments.Providers.Ics.Options;
using Microsoft.Extensions.Options;

namespace HairSalonAppointments.Providers.Ics.Providers;

public sealed class IcsProvider : ICalendarProvider
{
    private readonly IcsOptions _opt;
    public string Key => "ics";

    public IcsProvider(IOptions<IcsOptions> opt)
    {
        _opt = opt.Value;
        Directory.CreateDirectory(_opt.Directory);
    }

    public Task<string> CreateAsync(CalendarEventDto @event, CancellationToken cancellationToken = default)
    {
        var uid = Guid.NewGuid().ToString("N");
        var path = Path.Combine(_opt.Directory, $"{uid}.ics");

        string ToUtcStamp(DateTimeOffset dto) =>
            dto.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);

        var ics = $"""
                   BEGIN:VCALENDAR
                   VERSION:2.0
                   PRODID:-//HairSalonAppointments//ICSProvider//EN
                   BEGIN:VEVENT
                   UID:{uid}
                   SUMMARY:{@event.Title}
                   DTSTART:{ToUtcStamp(@event.Start)}
                   DTEND:{ToUtcStamp(@event.End)}
                   END:VEVENT
                   END:VCALENDAR
                   """;
        File.WriteAllText(path, ics);
        return Task.FromResult(uid);
    }

    public Task<IReadOnlyList<CalendarEventDto>> ListAsync(DateTimeOffset from, DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var list = new List<CalendarEventDto>();

        foreach (var file in Directory.EnumerateFiles(_opt.Directory, "*.ics"))
        {
            var lines = File.ReadAllLines(file);

            string? uid = null, summary = null, dtstart = null, dtend = null;

            foreach (var ln in lines)
            {
                if (ln.StartsWith("UID:", StringComparison.OrdinalIgnoreCase))
                {
                    uid = ln[4..];
                }
                else if (ln.StartsWith("SUMMARY:", StringComparison.OrdinalIgnoreCase))
                {
                    summary = ln[8..];
                }
                else if (ln.StartsWith("DTSTART:", StringComparison.OrdinalIgnoreCase))
                {
                    dtstart = ln[8..];
                }
                else if (ln.StartsWith("DTEND:", StringComparison.OrdinalIgnoreCase))
                {
                    dtend = ln[6..];
                }
            }

            if (uid is null || summary is null || dtstart is null || dtend is null)
            {
                continue;
            }

            if (!DateTimeOffset.TryParseExact(dtstart, "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var s))
            {
                continue;
            }

            if (!DateTimeOffset.TryParseExact(dtend, "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var e))
            {
                continue;
            }

            if (s < to && e > from)
            {
                list.Add(new CalendarEventDto(uid, summary, s, e, "UTC"));
            }
        }

        return Task.FromResult<IReadOnlyList<CalendarEventDto>>(list.OrderBy(x => x.Start).ToList());
    }
}