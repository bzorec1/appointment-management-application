using HairSalonAppointments.Contracts.Appointments;
using HairSalonAppointments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HairSalonAppointments.Tests.Unit;

public class AppointmentDataDataStoreTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AppointmentDataDataStore _sut;

    public AppointmentDataDataStoreTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _sut = new AppointmentDataDataStore(_dbContext);
    }

    [Fact]
    public async Task InsertAsync_ReturnsAppointmentWithId()
    {
        var appointment = CreateNewAppointment();

        var result = await _sut.InsertAsync(appointment);

        Assert.True(result.Id > 0);
        Assert.Equal(appointment.Title, result.Title);
        Assert.Equal(appointment.Service, result.Service);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsAppointment()
    {
        var inserted = await _sut.InsertAsync(CreateNewAppointment());

        var result = await _sut.GetByIdAsync(inserted.Id);

        Assert.NotNull(result);
        Assert.Equal(inserted.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task OverlapsAsync_OverlappingRange_ReturnsTrue()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(1);
        await _sut.InsertAsync(CreateNewAppointment(start: start, end: end, resourceId: 1));

        var overlaps = await _sut.OverlapsAsync(1, start.AddMinutes(30), end.AddMinutes(30));

        Assert.True(overlaps);
    }

    [Fact]
    public async Task OverlapsAsync_AdjacentAppointments_ReturnsFalse()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(1);
        await _sut.InsertAsync(CreateNewAppointment(start: start, end: end, resourceId: 1));

        var overlaps = await _sut.OverlapsAsync(1, end, end.AddHours(1));

        Assert.False(overlaps);
    }

    [Fact]
    public async Task OverlapsAsync_DifferentResource_ReturnsFalse()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(1);
        await _sut.InsertAsync(CreateNewAppointment(start: start, end: end, resourceId: 1));

        var overlaps = await _sut.OverlapsAsync(2, start, end);

        Assert.False(overlaps);
    }

    [Fact]
    public async Task OverlapsAsync_WithExcludeId_ExcludesSelf()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(1);
        var inserted = await _sut.InsertAsync(CreateNewAppointment(start: start, end: end, resourceId: 1));

        var overlaps = await _sut.OverlapsAsync(1, start, end, excludeId: inserted.Id);

        Assert.False(overlaps);
    }

    [Fact]
    public async Task UpdateAsync_ExistingAppointment_ReturnsUpdated()
    {
        var inserted = await _sut.InsertAsync(CreateNewAppointment());

        var updated = new NewAppointment(
            "Updated Title", inserted.Start, inserted.End,
            inserted.ResourceId, "999-9999", "color", "Updated Name");

        var result = await _sut.UpdateAsync(inserted.Id, updated);

        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("color", result.Service);
        Assert.Equal("Updated Name", result.CustomerName);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_ReturnsNull()
    {
        var result = await _sut.UpdateAsync(999, CreateNewAppointment());
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingAppointment_ReturnsTrue()
    {
        var inserted = await _sut.InsertAsync(CreateNewAppointment());

        var result = await _sut.DeleteAsync(inserted.Id);

        Assert.True(result);
        Assert.Null(await _sut.GetByIdAsync(inserted.Id));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ReturnsFalse()
    {
        var result = await _sut.DeleteAsync(999);
        Assert.False(result);
    }

    [Fact]
    public async Task InsertAsync_WithMultiPhaseFields_PopulatesCorrectly()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var activeEnd = start.AddMinutes(20);
        var passiveEnd = activeEnd.AddMinutes(40);

        var appointment = new NewAppointment(
            "Color", start, passiveEnd, 1, "555", "color", "Test",
            ServiceId: "color",
            ActiveStart: start,
            ActiveEnd: activeEnd,
            PassiveStart: activeEnd,
            PassiveEnd: passiveEnd);

        var result = await _sut.InsertAsync(appointment);

        Assert.NotNull(result.ActiveStart);
        Assert.NotNull(result.ActiveEnd);
        Assert.NotNull(result.PassiveStart);
        Assert.NotNull(result.PassiveEnd);
    }

    [Fact]
    public async Task ListAsync_ReturnsAppointmentsInRange()
    {
        var start = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        await _sut.InsertAsync(CreateNewAppointment(start: start, end: start.AddHours(1)));
        await _sut.InsertAsync(CreateNewAppointment(start: start.AddHours(2), end: start.AddHours(3)));

        var from = new DateTimeOffset(2025, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(1);

        var results = await _sut.ListAsync(from, to);

        Assert.Equal(2, results.Count);
    }

    private static NewAppointment CreateNewAppointment(
        DateTimeOffset? start = null,
        DateTimeOffset? end = null,
        int resourceId = 1)
    {
        var s = start ?? DateTimeOffset.UtcNow.AddHours(1);
        var e = end ?? s.AddHours(1);
        return new NewAppointment("Test Haircut", s, e, resourceId, "555-1234", "haircut", "Test User");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}