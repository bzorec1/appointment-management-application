namespace HairSalonAppointments.Providers.Google.Options;

public sealed record GoogleCalendarOptions
{
    public string? ServiceAccountKeyPath { get; set; }

    public string CalendarId { get; set; } = "primary";

    public string ApplicationName { get; set; } = "HairSalonAppointments";
}