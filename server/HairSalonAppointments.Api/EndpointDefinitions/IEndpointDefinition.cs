namespace HairSalonAppointments.Api.EndpointDefinitions;

internal interface IEndpointDefinition
{
    public void DefineEndpoints(RouteGroupBuilder api);

    public void DefineServices(IServiceCollection services);
}