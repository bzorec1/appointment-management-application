namespace HairSalonAppointments.Providers.Ics.Options;

public sealed record IcsOptions
{
    public string Directory { get; set; } = "./docs/ics";

    public string TimeZone { get; set; } = "UTC";
}