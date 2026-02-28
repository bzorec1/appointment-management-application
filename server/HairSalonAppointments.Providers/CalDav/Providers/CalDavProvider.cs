using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using HairSalonAppointments.Abstractions.Calendar;
using HairSalonAppointments.Contracts.Calendar;
using HairSalonAppointments.Providers.CalDav.Options;
using Microsoft.Extensions.Options;

namespace HairSalonAppointments.Providers.CalDav.Providers;

public sealed class CalDavProvider : ICalendarProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CalDavOptions _opt;
    public string Key => "caldav";

    public CalDavProvider(IHttpClientFactory httpClientFactory, IOptions<CalDavOptions> opt)
    {
        _httpClientFactory = httpClientFactory;
        _opt = opt.Value;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("CalDav");
        if (!string.IsNullOrWhiteSpace(_opt.Username))
        {
            var bytes = Encoding.ASCII.GetBytes($"{_opt.Username}:{_opt.Password}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
        }

        return client;
    }

    public async Task<string> CreateAsync(CalendarEventDto @event, CancellationToken cancellationToken = default)
    {
        var uid = Guid.NewGuid().ToString("N");

        var ics = $"""
                   BEGIN:VCALENDAR
                   VERSION:2.0
                   PRODID:-//HairSalonAppointments//CalDAV//EN
                   BEGIN:VEVENT
                   UID:{uid}
                   SUMMARY:{@event.Title}
                   DTSTART:{Dt(@event.Start)}
                   DTEND:{Dt(@event.End)}
                   END:VEVENT
                   END:VCALENDAR
                   """;

        var url = new Uri(new Uri(_opt.BaseUrl), $"{uid}.ics");
        var req = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(ics, Encoding.UTF8, "text/calendar")
        };

        using var client = CreateClient();
        using var resp = await client.SendAsync(req, cancellationToken);
        resp.EnsureSuccessStatusCode();
        return uid;

        string Dt(DateTimeOffset dto) => dto.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'");
    }

    public async Task<IReadOnlyList<CalendarEventDto>> ListAsync(DateTimeOffset from, DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        string Dt(DateTimeOffset dto) => dto.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'");

        var reportXml = $"""
                         <c:calendar-query xmlns:d="DAV:" xmlns:c="urn:ietf:params:xml:ns:caldav">
                           <d:prop><c:calendar-data/></d:prop>
                           <c:filter>
                             <c:comp-filter name="VCALENDAR">
                               <c:comp-filter name="VEVENT">
                                 <c:time-range start="{Dt(from)}" end="{Dt(to)}"/>
                               </c:comp-filter>
                             </c:comp-filter>
                           </c:filter>
                         </c:calendar-query>
                         """;

        var req = new HttpRequestMessage(new HttpMethod("REPORT"), _opt.BaseUrl)
        {
            Content = new StringContent(reportXml, Encoding.UTF8, "application/xml")
        };
        req.Headers.Add("Depth", "1");

        using var client = CreateClient();
        using var resp = await client.SendAsync(req, cancellationToken);
        resp.EnsureSuccessStatusCode();
        var xml = await resp.Content.ReadAsStringAsync(cancellationToken);

        var doc = XDocument.Parse(xml);
        var nsDav = XNamespace.Get("DAV:");
        var nsC = XNamespace.Get("urn:ietf:params:xml:ns:caldav");

        var events = new List<CalendarEventDto>();

        foreach (var propstat in doc.Descendants(nsDav + "propstat"))
        {
            var calData = propstat.Descendants(nsC + "calendar-data").FirstOrDefault();
            if (calData is null)
            {
                continue;
            }

            var ics = calData.Value;

            string? uid = null, summary = null, dtstart = null, dtend = null;
            foreach (var ln in ics.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (ln.StartsWith("UID:"))
                {
                    uid = ln[4..];
                }
                else if (ln.StartsWith("SUMMARY:"))
                {
                    summary = ln[8..];
                }
                else if (ln.StartsWith("DTSTART:"))
                {
                    dtstart = ln[8..];
                }
                else if (ln.StartsWith("DTEND:"))
                {
                    dtend = ln[6..];
                }
            }

            if (uid is null || summary is null || dtstart is null || dtend is null)
            {
                continue;
            }

            if (!DateTimeOffset.TryParseExact(dtstart, "yyyyMMdd'T'HHmmss'Z'", null,
                    System.Globalization.DateTimeStyles.AssumeUniversal, out var s))
            {
                continue;
            }

            if (!DateTimeOffset.TryParseExact(dtend, "yyyyMMdd'T'HHmmss'Z'", null,
                    System.Globalization.DateTimeStyles.AssumeUniversal, out var e))
            {
                continue;
            }

            events.Add(new CalendarEventDto(uid, summary, s, e, "UTC"));
        }

        return events.OrderBy(x => x.Start).ToList();
    }
}