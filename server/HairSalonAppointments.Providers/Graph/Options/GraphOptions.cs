namespace HairSalonAppointments.Providers.Graph.Options;

public sealed record GraphOptions
{
    public string TenantId { get; set; } = "";

    public string ClientId { get; set; } = "";

    public string ClientSecret { get; set; } = "";

    public string UserId { get; set; } = "";
}