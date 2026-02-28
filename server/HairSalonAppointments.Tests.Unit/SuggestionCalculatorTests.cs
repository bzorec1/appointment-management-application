using HairSalonAppointments.Abstractions;
using HairSalonAppointments.Abstractions.Appointments;
using HairSalonAppointments.Api.Services;
using HairSalonAppointments.Api.Suggestions;
using HairSalonAppointments.Api.Suggestions.Enums;
using HairSalonAppointments.Api.Suggestions.Requests;
using HairSalonAppointments.Contracts.Appointments;
using HairSalonAppointments.Contracts.Suggestions;
using NSubstitute;

namespace HairSalonAppointments.Tests.Unit;

public class SuggestionCalculatorTests
{
    private readonly IAppointmentDataStore _dataStore = Substitute.For<IAppointmentDataStore>();
    private readonly IServiceCatalog _serviceCatalog = Substitute.For<IServiceCatalog>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly SuggestionCalculator _sut;

    public SuggestionCalculatorTests()
    {
        _dateTimeProvider.Now.Returns(new DateTime(2025, 6, 14, 0, 0, 0, DateTimeKind.Utc));
        _sut = new SuggestionCalculator(_dataStore, _serviceCatalog, _dateTimeProvider);
    }

    [Fact]
    public async Task CalculateAsync_EmptyDay_ReturnsSuggestion()
    {
        var targetDate = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        _dataStore.ListAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<AppointmentDto>());

        var request = new CreateSuggestionRequest
        {
            TargetDate = targetDate,
            TimePreference = TimePreference.Morning,
            RequestedBy = RequestedBy.Client
        };

        var result = await _sut.CalculateAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotEmpty(result.Slots);
        Assert.True(result.Slots[0].StartUtc.Hour >= 9);
        Assert.True(result.Slots[0].EndUtc.Hour <= 12);
    }

    [Fact]
    public async Task CalculateAsync_WithServiceId_UsesServiceCatalogDuration()
    {
        var targetDate = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        _dataStore.ListAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<AppointmentDto>());

        _serviceCatalog.GetService("color")
            .Returns(new HairSalonAppointments.Contracts.Services.ServiceDefinition(
                "color", "Hair Coloring", TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(40), true));

        var request = new CreateSuggestionRequest
        {
            TargetDate = targetDate,
            TimePreference = TimePreference.Morning,
            RequestedBy = RequestedBy.Client,
            ServiceId = "color"
        };

        var result = await _sut.CalculateAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotEmpty(result.Slots);
        // Duration should be 60 minutes (20 active + 40 passive)
        var duration = result.Slots[0].EndUtc - result.Slots[0].StartUtc;
        Assert.Equal(TimeSpan.FromMinutes(60), duration);
    }

    [Fact]
    public async Task CalculateAsync_FullyBookedDay_ReturnsFailure()
    {
        var targetDate = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        // Return a fully-booked day for whatever date is queried (calculator scans 14 days forward)
        _dataStore.ListAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var from = callInfo.ArgAt<DateTimeOffset>(0);
                var day = from.UtcDateTime.Date;
                var booked = new List<AppointmentDto>();
                for (int hour = 9; hour < 18; hour++)
                    for (int min = 0; min < 60; min += 15)
                    {
                        var s = new DateTimeOffset(day.Year, day.Month, day.Day, hour, min, 0, TimeSpan.Zero);
                        booked.Add(new AppointmentDto(
                            booked.Count + 1, "Test", s, s.AddMinutes(15),
                            1, "Ana", "555", "haircut", "Test", s));
                    }
                return Task.FromResult<IReadOnlyList<AppointmentDto>>(booked);
            });

        var request = new CreateSuggestionRequest
        {
            TargetDate = targetDate,
            TimePreference = TimePreference.Morning,
            RequestedBy = RequestedBy.Client
        };

        var result = await _sut.CalculateAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task CalculateAsync_AfternoonPreference_ReturnsAfternoonSlot()
    {
        var targetDate = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        _dataStore.ListAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<AppointmentDto>());

        var request = new CreateSuggestionRequest
        {
            TargetDate = targetDate,
            TimePreference = TimePreference.Afternoon,
            RequestedBy = RequestedBy.Client
        };

        var result = await _sut.CalculateAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotEmpty(result.Slots);
        Assert.True(result.Slots[0].StartUtc.Hour >= 12);
        Assert.True(result.Slots[0].EndUtc.Hour <= 17);
    }
}