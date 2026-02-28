namespace HairSalonAppointments.Providers.Graph.Options;

public sealed class GraphOptions
{
    /// <summary>Azure AD tenant (Directory) ID. Found in Azure Portal → App registrations → your app → Overview.</summary>
    public string TenantId { get; set; } = "";

    /// <summary>Application (client) ID. Found in Azure Portal → App registrations → your app → Overview.</summary>
    public string ClientId { get; set; } = "";

    /// <summary>Client secret value. Azure Portal → App registrations → Certificates &amp; secrets → New client secret.</summary>
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// The user's Object ID or UPN (email) whose calendar to use.
    /// Azure Portal → Users → select user → Object ID.
    /// The app registration needs 'Calendars.ReadWrite' application permission granted by an admin.
    /// </summary>
    public string UserId { get; set; } = "";
}
