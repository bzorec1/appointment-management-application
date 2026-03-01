using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using HairSalonAppointments.Abstractions.Calendar;
using HairSalonAppointments.Contracts.Calendar;
using HairSalonAppointments.Providers.Google.Options;
using Microsoft.Extensions.Options;

namespace HairSalonAppointments.Providers.Google.Providers;

public sealed class GoogleCalendarProvider(
    IOptions<GoogleCalendarOptions> options) : ICalendarProvider
{
    private readonly GoogleCalendarOptions _options = options.Value;

    public string Key => "google";

    private CalendarService CreateService()
    {
        if (string.IsNullOrEmpty(_options.ServiceAccountKeyPath))
        {
            throw new InvalidOperationException(
                "Google Calendar service account key path not configured. " +
                "Set the 'Google:ServiceAccountKeyPath' configuration.");
        }

        GoogleCredential credential;
        using (var stream = new FileStream(_options.ServiceAccountKeyPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(CalendarService.Scope.Calendar);
        }

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName
        });
    }

    public async Task<string> CreateAsync(
        CalendarEventDto @event,
        CancellationToken cancellationToken = default)
    {
        var service = CreateService();

        var googleEvent = new Event
        {
            Summary = @event.Title,
            Description = @event.Notes,
            ColorId = @event.ColorId,
            Start = new EventDateTime
            {
                DateTimeDateTimeOffset = @event.Start,
                TimeZone = @event.TimeZone
            },
            End = new EventDateTime
            {
                DateTimeDateTimeOffset = @event.End,
                TimeZone = @event.TimeZone
            }
        };

        var request = service.Events.Insert(googleEvent, _options.CalendarId);
        var created = await request.ExecuteAsync(cancellationToken);
        return created.Id;
    }

    public async Task<IReadOnlyList<CalendarEventDto>> ListAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var service = CreateService();

        var request = service.Events.List(_options.CalendarId);
        request.TimeMinDateTimeOffset = from;
        request.TimeMaxDateTimeOffset = to;
        request.SingleEvents = true;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        var response = await request.ExecuteAsync(cancellationToken);

        return response.Items == null
            ? []
            : response.Items
                .Select(e => new CalendarEventDto(
                    e.Id,
                    e.Summary ?? "Untitled",
                    e.Start.DateTimeDateTimeOffset ?? DateTimeOffset.Parse(e.Start.Date),
                    e.End.DateTimeDateTimeOffset ?? DateTimeOffset.Parse(e.End.Date),
                    e.Start.TimeZone ?? "UTC",
                    e.Description
                ))
                .ToList();
    }
}