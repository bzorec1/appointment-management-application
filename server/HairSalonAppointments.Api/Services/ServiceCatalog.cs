using HairSalonAppointments.Contracts.Services;

namespace HairSalonAppointments.Api.Services;

public interface IServiceCatalog
{
    ServiceDefinition? GetService(string id);
    IReadOnlyList<ServiceDefinition> GetAllServices();
}

public sealed class ServiceCatalog : IServiceCatalog
{
    private static readonly List<ServiceDefinition> Services =
    [
        new("haircut", "Haircut", TimeSpan.FromMinutes(30), TimeSpan.Zero, false),
        new("haircut-wash", "Haircut + Wash", TimeSpan.FromMinutes(45), TimeSpan.Zero, false),
        new("color", "Hair Coloring", TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(40), true),
        new("highlights", "Highlights", TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(45), true),
        new("perm", "Perm", TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(30), true),
        new("blowdry", "Blow Dry", TimeSpan.FromMinutes(20), TimeSpan.Zero, false),
        new("beard", "Beard Trim", TimeSpan.FromMinutes(15), TimeSpan.Zero, false)
    ];

    public ServiceDefinition? GetService(string id)
        => Services.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<ServiceDefinition> GetAllServices() => Services;
}
