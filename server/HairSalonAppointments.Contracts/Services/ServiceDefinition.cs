namespace HairSalonAppointments.Contracts.Services;

public record ServiceDefinition(
    string Id,
    string Name,
    TimeSpan ActiveDuration,
    TimeSpan PassiveDuration,
    bool AllowsNesting);
