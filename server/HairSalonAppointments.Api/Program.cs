using Asp.Versioning;
using HairSalonAppointments.Api.EndpointDefinitions;
using HairSalonAppointments.Api.Infrastructure;
using HairSalonAppointments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;

namespace HairSalonAppointments.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true,
                reloadOnChange: true)
            .AddEnvironmentVariables();

        Log.Logger = builder.CreateLogger();
        builder.Host.UseSerilog();

        builder.Services.AddHttpJsonOptions();
        builder.Services.AddCorsFromConfig(builder.Configuration);
        builder.Services.AddApiVersioningAndExplorer();

        builder.Services.AddApiServices(builder.Configuration);

        builder.Services.AddTransient<IEndpointDefinition, AppointmentEndpointDefinition>();
        builder.Services.AddTransient<IEndpointDefinition, SuggestionsEndpointDefinition>();
        builder.Services.AddTransient<IEndpointDefinition, ServicesEndpointDefinition>();

        builder.Services.AddMemoryCache();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            await scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>()
                .Database.MigrateAsync()
                .ConfigureAwait(false);
        }

        app.UseForwardedHeaders();

        app.UseSerilogRequestLogging();

        app.UseRequestLocalization(
            app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        if (app.Environment.IsProduction())
        {
            app.UseHsts();
        }

        app.UseCors(CorsExtensions.DefaultPolicyName);

        var versionSet = app
            .NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var apiV1 = app
            .MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0))
;

        app.MapEndpointDefinitions(apiV1);

        if (app.Environment.IsDevelopment())
        {
            app
                .MapOpenApi()
                .AllowAnonymous();

            app
                .MapScalarApiReference()
                .WithDisplayName("HairSalon API")
                .AllowAnonymous();
        }

        await app
            .RunAsync()
            .ConfigureAwait(false);
    }
}