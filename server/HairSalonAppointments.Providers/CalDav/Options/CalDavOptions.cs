namespace HairSalonAppointments.Providers.CalDav.Options;

public sealed class CalDavOptions
{
    public string BaseUrl { get; set; } = "";

    public string? Username { get; set; }

    public string? Password { get; set; }
}