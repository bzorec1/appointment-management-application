using System.Globalization;
using System.Threading.RateLimiting;
using HairSalonAppointments.Abstractions;
using HairSalonAppointments.Abstractions.Appointments;
using HairSalonAppointments.Abstractions.Calendar;
using HairSalonAppointments.Api.EndpointDefinitions;
using HairSalonAppointments.Api.Scheduling;
using HairSalonAppointments.Api.Services;
using HairSalonAppointments.Api.Suggestions;
using HairSalonAppointments.Infrastructure.Persistence;
using HairSalonAppointments.Providers.CalDav.Options;
using HairSalonAppointments.Providers.CalDav.Providers;
using HairSalonAppointments.Providers.Core;
using HairSalonAppointments.Providers.Google.Options;
using HairSalonAppointments.Providers.Google.Providers;
using HairSalonAppointments.Providers.Graph.Options;
using HairSalonAppointments.Providers.Graph.Providers;
using HairSalonAppointments.Providers.Ics.Options;
using HairSalonAppointments.Providers.Ics.Providers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;
using ApplicationDbContext = HairSalonAppointments.Infrastructure.Persistence.ApplicationDbContext;

namespace HairSalonAppointments.Api.Infrastructure;

internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(o =>
            o.UseSqlite(config.GetConnectionString("sqlite") ?? "Data Source=./hair.db"));

        services.AddScoped<IAppointmentDataStore, AppointmentDataDataStore>();
        services.AddScoped<ISchedulingService, SchedulingService>();
        services.AddScoped<ISuggestionCalculator, SuggestionCalculator>();

        services.AddSingleton<IServiceCatalog, ServiceCatalog>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.Configure<IcsOptions>(config.GetSection("Ics"));
        services.Configure<CalDavOptions>(config.GetSection("CalDav"));
        services.Configure<GoogleCalendarOptions>(config.GetSection("Google"));
        services.Configure<GraphOptions>(config.GetSection("Graph"));

        services.AddSingleton<IcsProvider>();
        services.AddSingleton<GoogleCalendarProvider>();
        services.AddSingleton<CalDavProvider>();
        services.AddSingleton<GraphCalendarProvider>();

        services.AddSingleton<ICalendarProvider>(sp => sp.GetRequiredService<IcsProvider>());
        services.AddSingleton<ICalendarProvider>(sp => sp.GetRequiredService<GoogleCalendarProvider>());
        services.AddSingleton<ICalendarProvider>(sp => sp.GetRequiredService<CalDavProvider>());
        services.AddSingleton<ICalendarProvider>(sp => sp.GetRequiredService<GraphCalendarProvider>());

        services.AddSingleton<ICalendarProviderResolver, CalendarProviderResolver>();

        services.AddOpenApi();
        services.AddHttpClient();
        services.AddRateLimiter(o =>
        {
            o.AddPolicy("health-details", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 12,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    public static WebApplication MapEndpointDefinitions(
        this WebApplication app,
        RouteGroupBuilder root)
    {
        var definitions = app.Services.GetServices<IEndpointDefinition>();
        foreach (var definition in definitions)
        {
            definition.DefineEndpoints(root);
        }

        return app;
    }

    public static Logger CreateLogger(this WebApplicationBuilder builder)
    {
        var isDev = builder.Environment.IsDevelopment();

        var config = new LoggerConfiguration()
            .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath",
                p => p.StartsWith("/api/v1/health", StringComparison.OrdinalIgnoreCase)))
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId();

        if (isDev)
        {
            config.MinimumLevel.Debug();
        }
        else
        {
            config.MinimumLevel.Information();
        }

        config
            .MinimumLevel.Override("Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersMiddleware", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);

        config.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
            theme: AnsiConsoleTheme.Code,
            formatProvider: CultureInfo.InvariantCulture
        );

        var logToFile = Environment.GetEnvironmentVariable("LOG_TO_FILE");
        if (string.Equals(logToFile, "true", StringComparison.OrdinalIgnoreCase))
        {
            config.WriteTo.File(
                new CompactJsonFormatter(),
                path: "logs/log-.json",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information
            );
        }

        return config.CreateLogger();
    }
}