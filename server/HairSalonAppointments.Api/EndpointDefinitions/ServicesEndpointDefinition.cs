using HairSalonAppointments.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HairSalonAppointments.Api.EndpointDefinitions;

public sealed class ServicesEndpointDefinition : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
    }

    public void DefineEndpoints(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/services");

        group
            .MapGet("", GetAllServices)
            .AllowAnonymous();

        group
            .MapGet("{serviceId}", GetService)
            .AllowAnonymous();
    }

    private static IResult GetAllServices([FromServices] IServiceCatalog catalog)
        => Results.Ok(catalog.GetAllServices());

    private static IResult GetService(string serviceId, [FromServices] IServiceCatalog catalog)
    {
        var service = catalog.GetService(serviceId);
        return service is null ? Results.NotFound() : Results.Ok(service);
    }
}
