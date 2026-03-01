namespace HairSalonAppointments.Contracts.Services;

public sealed record ServiceDefinition(
    string Id,
    string Name,
    TimeSpan ActiveDuration,
    TimeSpan PassiveDuration,
    bool AllowsNesting);