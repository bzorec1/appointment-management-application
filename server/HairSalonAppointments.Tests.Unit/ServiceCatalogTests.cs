using HairSalonAppointments.Api.Services;

namespace HairSalonAppointments.Tests.Unit;

public class ServiceCatalogTests
{
    private readonly ServiceCatalog _sut = new();

    [Fact]
    public void GetAllServices_ReturnsNonEmptyList()
    {
        var services = _sut.GetAllServices();
        Assert.NotEmpty(services);
    }

    [Theory]
    [InlineData("haircut")]
    [InlineData("color")]
    [InlineData("highlights")]
    [InlineData("beard")]
    public void GetService_KnownId_ReturnsDefinition(string serviceId)
    {
        var service = _sut.GetService(serviceId);
        Assert.NotNull(service);
        Assert.Equal(serviceId, service.Id);
        Assert.True(service.ActiveDuration > TimeSpan.Zero);
    }

    [Fact]
    public void GetService_UnknownId_ReturnsNull()
    {
        var service = _sut.GetService("nonexistent");
        Assert.Null(service);
    }

    [Fact]
    public void GetService_IsCaseInsensitive()
    {
        var service = _sut.GetService("HAIRCUT");
        Assert.NotNull(service);
        Assert.Equal("haircut", service.Id);
    }

    [Fact]
    public void MultiPhaseService_HasPassiveDuration()
    {
        var color = _sut.GetService("color");
        Assert.NotNull(color);
        Assert.True(color.PassiveDuration > TimeSpan.Zero);
        Assert.True(color.AllowsNesting);
    }

    [Fact]
    public void SinglePhaseService_HasZeroPassiveDuration()
    {
        var haircut = _sut.GetService("haircut");
        Assert.NotNull(haircut);
        Assert.Equal(TimeSpan.Zero, haircut.PassiveDuration);
        Assert.False(haircut.AllowsNesting);
    }
}
