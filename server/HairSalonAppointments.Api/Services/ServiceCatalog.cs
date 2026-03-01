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
        new(Id: "haircut",
            Name: "Haircut",
            ActiveDuration: TimeSpan.FromMinutes(minutes: 30),
            PassiveDuration: TimeSpan.Zero,
            AllowsNesting: false),
        new(Id: "haircut-wash",
            Name: "Haircut + Wash",
            ActiveDuration: TimeSpan.FromMinutes(minutes: 45),
            PassiveDuration: TimeSpan.Zero,
            AllowsNesting: false),
        new(Id: "color",
            Name: "Hair Coloring",
            ActiveDuration: TimeSpan.FromMinutes(minutes: 20),
            PassiveDuration: TimeSpan.FromMinutes(minutes: 40),
            AllowsNesting: true),
        new(Id: "highlights",
            Name: "Highlights",
            ActiveDuration: TimeSpan.FromMinutes(minutes: 30),
            PassiveDuration: TimeSpan.FromMinutes(minutes: 45),
            AllowsNesting: true),
        new(Id: "perm",
            Name: "Perm",
            ActiveDuration: TimeSpan.FromMinutes(minutes: 25),
            PassiveDuration: TimeSpan.FromMinutes(minutes: 30),
            AllowsNesting: true),
        new(Id: "blowdry",
            Name: "Blow Dry",
            ActiveDuration: TimeSpan.FromMinutes(minutes: 20),
            PassiveDuration: TimeSpan.Zero,
            AllowsNesting: false),
        new(Id: "beard",
            Name: "Beard Trim",
            ActiveDuration: TimeSpan.FromMinutes(minutes: 15),
            PassiveDuration: TimeSpan.Zero,
            AllowsNesting: false)
    ];

    public ServiceDefinition? GetService(string id)
    {
        return Services.FirstOrDefault(s =>
            s.Id.Equals(value: id, comparisonType: StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<ServiceDefinition> GetAllServices() => Services;
}