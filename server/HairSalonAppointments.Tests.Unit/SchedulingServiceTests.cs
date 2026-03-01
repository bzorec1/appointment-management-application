using HairSalonAppointments.Abstractions.Appointments;
using HairSalonAppointments.Abstractions.Calendar;
using HairSalonAppointments.Api.Scheduling;
using HairSalonAppointments.Api.Services;
using HairSalonAppointments.Contracts.Appointments;
using HairSalonAppointments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HairSalonAppointments.Tests.Unit;

public class SchedulingServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAppointmentDataStore _dataStore;
    private readonly ICalendarProviderResolver _calendarResolver;
    private readonly IServiceCatalog _serviceCatalog;
    private readonly SchedulingService _sut;

    public SchedulingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _dataStore = Substitute.For<IAppointmentDataStore>();
        _calendarResolver = Substitute.For<ICalendarProviderResolver>();
        _serviceCatalog = Substitute.For<IServiceCatalog>();
        var logger = Substitute.For<ILogger<SchedulingService>>();

        var provider = Substitute.For<ICalendarProvider>();
        provider.CreateAsync(Arg.Any<Contracts.Calendar.CalendarEventDto>(),
                Arg.Any<CancellationToken>())
            .Returns("event-id");
        _calendarResolver.Get("google").Returns(provider);

        _sut = new SchedulingService(_dataStore, _calendarResolver, _serviceCatalog, _dbContext, logger);
    }

    [Fact]
    public async Task CreateAsync_ValidAppointment_SavesAndSyncsToCalendar()
    {
        var appointment = new NewAppointment(
            "Haircut - Ana",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(2),
            1, "555-1234", "haircut", "Ana");

        _dataStore.OverlapsAsync(Arg.Any<int>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var savedDto = new AppointmentDto(1, appointment.Title, appointment.Start, appointment.End,
            appointment.ResourceId, null, appointment.Phone, appointment.Service,
            appointment.CustomerName, DateTimeOffset.UtcNow);
        _dataStore.InsertAsync(Arg.Any<NewAppointment>(), Arg.Any<CancellationToken>())
            .Returns(savedDto);

        var result = await _sut.CreateAsync(appointment);

        Assert.Equal(savedDto, result);
        await _dataStore.Received(1).InsertAsync(Arg.Any<NewAppointment>(), Arg.Any<CancellationToken>());
        _calendarResolver.Received(1).Get("google");
    }

    [Fact]
    public async Task CreateAsync_Overlap_ThrowsInvalidOperationException()
    {
        var appointment = new NewAppointment(
            "Haircut", DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2),
            1, "555", "haircut", "Test");

        _dataStore.OverlapsAsync(Arg.Any<int>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(appointment));
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        var appointment = new NewAppointment(
            "", DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2),
            1, "555", "haircut", "Test");

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(appointment));
    }

    [Fact]
    public async Task CreateAsync_EndBeforeStart_ThrowsArgumentException()
    {
        var appointment = new NewAppointment(
            "Test", DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow.AddHours(1),
            1, "555", "haircut", "Test");

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(appointment));
    }

    [Fact]
    public async Task CreateAsync_CalendarSyncFailure_StillReturnsAppointment()
    {
        var appointment = new NewAppointment(
            "Haircut", DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2),
            1, "555", "haircut", "Test");

        _dataStore.OverlapsAsync(Arg.Any<int>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var savedDto = new AppointmentDto(1, "Haircut", appointment.Start, appointment.End,
            1, null, "555", "haircut", "Test", DateTimeOffset.UtcNow);
        _dataStore.InsertAsync(Arg.Any<NewAppointment>(), Arg.Any<CancellationToken>())
            .Returns(savedDto);

        var provider = Substitute.For<ICalendarProvider>();
        provider.CreateAsync(Arg.Any<HairSalonAppointments.Contracts.Calendar.CalendarEventDto>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Google API down"));
        _calendarResolver.Get("google").Returns(provider);

        var result = await _sut.CreateAsync(appointment);

        Assert.Equal(savedDto, result);
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToDataStore()
    {
        _dataStore.DeleteAsync(42, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.DeleteAsync(42);

        Assert.True(result);
        await _dataStore.Received(1).DeleteAsync(42, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ValidAppointment_ReturnsUpdated()
    {
        var appointment = new NewAppointment(
            "Updated", DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2),
            1, "555", "haircut", "Test");

        _dataStore.OverlapsAsync(Arg.Any<int>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
                Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var updatedDto = new AppointmentDto(1, "Updated", appointment.Start, appointment.End,
            1, null, "555", "haircut", "Test", DateTimeOffset.UtcNow);
        _dataStore.UpdateAsync(1, Arg.Any<NewAppointment>(), Arg.Any<CancellationToken>())
            .Returns(updatedDto);

        var result = await _sut.UpdateAsync(1, appointment);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Title);
        await _dataStore.Received(1).UpdateAsync(1, Arg.Any<NewAppointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Overlap_ThrowsInvalidOperationException()
    {
        var appointment = new NewAppointment(
            "Test", DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2),
            1, "555", "haircut", "Test");

        _dataStore.OverlapsAsync(Arg.Any<int>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
                Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(1, appointment));
    }

    [Fact]
    public async Task CreateAsync_WithMultiPhaseService_EnrichesPhases()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var appointment = new NewAppointment(
            "Color", start, start.AddMinutes(60),
            1, "555", "color", "Test", ServiceId: "color");

        _serviceCatalog.GetService("color")
            .Returns(new HairSalonAppointments.Contracts.Services.ServiceDefinition(
                "color", "Hair Coloring", TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(40), true));

        _dataStore.OverlapsAsync(Arg.Any<int>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        NewAppointment? captured = null;
        _dataStore.InsertAsync(Arg.Do<NewAppointment>(a => captured = a), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var a = callInfo.Arg<NewAppointment>();
                return new AppointmentDto(1, a.Title, a.Start, a.End, a.ResourceId, null,
                    a.Phone, a.Service, a.CustomerName, DateTimeOffset.UtcNow,
                    ActiveStart: a.ActiveStart, ActiveEnd: a.ActiveEnd,
                    PassiveStart: a.PassiveStart, PassiveEnd: a.PassiveEnd);
            });

        var result = await _sut.CreateAsync(appointment);

        Assert.NotNull(captured);
        Assert.Equal(start, captured.ActiveStart);
        Assert.Equal(start.AddMinutes(20), captured.ActiveEnd);
        Assert.Equal(start.AddMinutes(20), captured.PassiveStart);
        Assert.Equal(start.AddMinutes(60), captured.PassiveEnd);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}