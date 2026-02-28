namespace HairSalonAppointments.Providers.CalDav.Options;

public sealed class CalDavOptions
{
    /// <summary>Base URL to calendar collection, e.g. https://localhost:5232/user/calendar/</summary>
    public string BaseUrl { get; set; } = "";

    public string? Username { get; set; }

    public string? Password { get; set; }
}