using Azure.Identity;
using HairSalonAppointments.Abstractions.Calendar;
using HairSalonAppointments.Contracts.Calendar;
using HairSalonAppointments.Providers.Graph.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace HairSalonAppointments.Providers.Graph.Providers;

public sealed class GraphCalendarProvider : ICalendarProvider
{
    private readonly GraphOptions _options;

    public string Key => "graph";

    public GraphCalendarProvider(IOptions<GraphOptions> options)
    {
        _options = options.Value;
    }

    private GraphServiceClient CreateClient()
    {
        if (string.IsNullOrEmpty(_options.TenantId) ||
            string.IsNullOrEmpty(_options.ClientId) ||
            string.IsNullOrEmpty(_options.ClientSecret))
        {
            throw new InvalidOperationException(
                "Microsoft Graph credentials not configured. " +
                "Set 'Graph:TenantId', 'Graph:ClientId', and 'Graph:ClientSecret' in configuration.");
        }

        var credential = new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret);
        return new GraphServiceClient(credential);
    }

    public async Task<string> CreateAsync(CalendarEventDto @event, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();

        var requestBody = new Event
        {
            Subject = @event.Title,
            Body = new ItemBody { Content = @event.Notes, ContentType = BodyType.Text },
            Start = new DateTimeTimeZone
            {
                DateTime = @event.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                TimeZone = @event.TimeZone
            },
            End = new DateTimeTimeZone
            {
                DateTime = @event.End.ToString("yyyy-MM-ddTHH:mm:ss"),
                TimeZone = @event.TimeZone
            }
        };

        var created = await client.Users[_options.UserId].Events
            .PostAsync(requestBody, cancellationToken: cancellationToken);

        return created?.Id ?? throw new InvalidOperationException("Graph API returned no event ID.");
    }

    public async Task<IReadOnlyList<CalendarEventDto>> ListAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient();

        var result = await client.Users[_options.UserId].CalendarView.GetAsync(config =>
        {
            config.QueryParameters.StartDateTime = from.ToString("yyyy-MM-ddTHH:mm:ss");
            config.QueryParameters.EndDateTime = to.ToString("yyyy-MM-ddTHH:mm:ss");
        }, cancellationToken: cancellationToken);

        if (result?.Value is null)
            return [];

        return result.Value
            .Select(e => new CalendarEventDto(
                e.Id,
                e.Subject ?? "Untitled",
                e.Start is not null ? DateTimeOffset.Parse(e.Start.DateTime!) : DateTimeOffset.UtcNow,
                e.End is not null ? DateTimeOffset.Parse(e.End.DateTime!) : DateTimeOffset.UtcNow,
                e.Start?.TimeZone ?? "UTC",
                e.Body?.Content
            ))
            .ToList();
    }
}
