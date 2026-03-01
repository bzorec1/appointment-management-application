using HairSalonAppointments.Abstractions;
using HairSalonAppointments.Api.Suggestions;
using HairSalonAppointments.Contracts.Suggestions;
using HairSalonAppointments.Contracts.Suggestions.Requests;
using HairSalonAppointments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HairSalonAppointments.Api.EndpointDefinitions;

public sealed class SuggestionsEndpointDefinition : IEndpointDefinition
{
    private const int SuggestionExpiryMinutes = 5;

    public void DefineServices(IServiceCollection services)
    {
    }

    public void DefineEndpoints(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/suggestions");

        group
            .MapGet("{suggestionId:guid}", GetSuggestions)
            .AllowAnonymous();

        group
            .MapPost("", CreateSuggestion)
            .AllowAnonymous();
    }

    private static async Task<IResult> CreateSuggestion(
        [FromBody] CreateSuggestionRequest request,
        [FromServices] ISuggestionCalculator calculator,
        [FromServices] IDateTimeProvider dateTimeProvider,
        [FromServices] ApplicationDbContext context,
        [FromServices] ILogger<SuggestionsEndpointDefinition> logger,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return Results.BadRequest();
        }

        if (request.TargetDate.HasValue && request.TargetDate.Value.Date < dateTimeProvider.Today)
        {
            return Results.BadRequest("Target date cannot be in the past.");
        }

        var suggestionResult = await calculator.CalculateAsync(request, cancellationToken);

        if (!suggestionResult.Success || suggestionResult.Slots.Count == 0)
        {
            return Results.BadRequest(suggestionResult.ErrorMessage ?? "No available slots found.");
        }

        var primary = suggestionResult.Slots[0];
        var now = dateTimeProvider.Now;
        var entity = new SuggestionEntity
        {
            Id = Guid.NewGuid(),
            SuggestedStartUtc = primary.StartUtc,
            SuggestedEndUtc = primary.EndUtc,
            State = SuggestionState.Suggested,
            RequestedBy = request.RequestedBy,
            CreatedAtUtc = now,
            PromotedAtUtc = null
        };

        try
        {
            await context.Suggestions.AddAsync(entity, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving the suggestion.");
            return Results.Problem("Failed to create suggestion.");
        }

        logger.LogInformation(
            "Created suggestion {suggestionId} with {slotCount} slot(s), primary Start {suggestedStart}",
            entity.Id,
            suggestionResult.Slots.Count,
            entity.SuggestedStartUtc);

        return Results.Created(
            $"/suggestions/{entity.Id}",
            new
            {
                entity.Id,
                ExpiresAt = now.AddMinutes(SuggestionExpiryMinutes),
                Slots = suggestionResult.Slots.Select(s => new { s.StartUtc, s.EndUtc }).ToArray()
            });
    }

    private static async Task<IResult> GetSuggestions(
        Guid suggestionId,
        [FromServices] IDateTimeProvider dateTimeProvider,
        [FromServices] ApplicationDbContext context,
        CancellationToken cancellationToken = default)
    {
        var suggestion = await context.Suggestions
            .FirstOrDefaultAsync(s => s.Id == suggestionId, cancellationToken);

        if (suggestion == null)
        {
            return Results.NotFound("Suggestion not found.");
        }

        if (suggestion.State == SuggestionState.Created &&
            suggestion.CreatedAtUtc.AddMinutes(SuggestionExpiryMinutes) < dateTimeProvider.Now)
        {
            return Results.BadRequest("Suggestion has expired.");
        }

        if (suggestion.State != SuggestionState.Created)
        {
            return Results.BadRequest("Suggestion has already been promoted to an appointment.");
        }

        return Results.Ok(new
        {
            suggestion.Id,
            suggestion.SuggestedStartUtc,
            suggestion.SuggestedEndUtc,
            suggestion.CreatedAtUtc,
            ExpiresAt = suggestion.CreatedAtUtc.AddMinutes(SuggestionExpiryMinutes),
            suggestion.RequestedBy
        });
    }
}