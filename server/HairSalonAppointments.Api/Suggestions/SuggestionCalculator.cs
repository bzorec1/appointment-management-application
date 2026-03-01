using HairSalonAppointments.Abstractions;
using HairSalonAppointments.Abstractions.Appointments;
using HairSalonAppointments.Api.Models;
using HairSalonAppointments.Api.Services;
using HairSalonAppointments.Api.Suggestions.Models;
using HairSalonAppointments.Contracts.Suggestions;
using HairSalonAppointments.Contracts.Suggestions.Requests;

namespace HairSalonAppointments.Api.Suggestions;

public interface ISuggestionCalculator
{
    Task<SuggestionResult> CalculateAsync(
        CreateSuggestionRequest request,
        CancellationToken cancellationToken);
}

public sealed class SuggestionCalculator(
    IAppointmentDataStore appointmentDataStore,
    IServiceCatalog serviceCatalog,
    IDateTimeProvider dateTimeProvider) : ISuggestionCalculator
{
    public async Task<SuggestionResult> CalculateAsync(
        CreateSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        var baseDate = DetermineBaseDate(request);

        if (!request.TargetDate.HasValue &&
            request.PreferredTime.HasValue)
        {
            return await CollectSlotsAcrossDaysAsync(
                baseDate,
                request,
                days: 5,
                slotsPerDay: 4,
                cancellationToken);
        }

        for (var i = 0; i < 14; i++)
        {
            var candidate = baseDate.Date.AddDays(i);
            var context = await LoadSchedulingContextAsync(
                candidate,
                request.ServiceId,
                cancellationToken);

            var slots = FindAvailableSlotsForDay(
                candidate,
                request.TimePreference,
                request.PreferredTime,
                context,
                maxSlots: 4);

            if (slots.Count > 0)
            {
                return SuggestionResult.Ok(slots);
            }
        }

        return SuggestionResult.Fail("Ni prostih terminov v naslednjih 14 dneh.");
    }

    private async Task<SuggestionResult> CollectSlotsAcrossDaysAsync(
        DateTime from,
        CreateSuggestionRequest request,
        int days,
        int slotsPerDay,
        CancellationToken cancellationToken)
    {
        var slots = new List<TimeSlot>();
        for (var i = 0; i < days; i++)
        {
            var candidate = from.Date.AddDays(i);
            var ctx = await LoadSchedulingContextAsync(
                candidate,
                request.ServiceId,
                cancellationToken);

            var daySlots = FindAvailableSlotsForDay(
                candidate,
                request.TimePreference,
                request.PreferredTime,
                ctx,
                maxSlots: slotsPerDay);

            slots.AddRange(daySlots);
        }

        return slots.Count == 0
            ? SuggestionResult.Fail("Ni prostih terminov v naslednjih 5 dneh ob izbranem času.")
            : SuggestionResult.Ok(slots);
    }

    private DateTime DetermineBaseDate(CreateSuggestionRequest request)
    {
        return request.TargetDate ?? dateTimeProvider.Now;
    }

    private async Task<SchedulingContext> LoadSchedulingContextAsync(
        DateTime baseDate,
        string? serviceId,
        CancellationToken cancellationToken)
    {
        var from = new DateTimeOffset(baseDate.Date, TimeSpan.Zero);
        var to = from.AddDays(1);

        var appointments = await appointmentDataStore.ListAsync(
            from,
            to,
            cancellationToken);

        var existingAppointments = appointments
            .Select(a => new Appointment
            {
                Id = Guid.NewGuid(),
                StartUtc = a.Start.UtcDateTime,
                EndUtc = a.End.UtcDateTime
            })
            .ToArray();

        var duration = TimeSpan.FromMinutes(30);

        if (string.IsNullOrEmpty(serviceId))
        {
            return new SchedulingContext
            {
                ExistingAppointments = existingAppointments,
                WorkingHours =
                [
                    new WorkingHours
                    {
                        Start = TimeSpan.FromHours(9),
                        End = TimeSpan.FromHours(18)
                    }
                ],
                Breaks = [],
                ServiceDuration = duration
            };
        }

        var service = serviceCatalog.GetService(serviceId);
        if (service != null)
        {
            duration = service.ActiveDuration + service.PassiveDuration;
        }

        return new SchedulingContext
        {
            ExistingAppointments = existingAppointments,
            WorkingHours =
            [
                new WorkingHours
                {
                    Start = TimeSpan.FromHours(9),
                    End = TimeSpan.FromHours(18)
                }
            ],
            Breaks = [],
            ServiceDuration = duration
        };
    }

    private IReadOnlyList<TimeSlot> FindAvailableSlotsForDay(
        DateTime baseDate,
        TimePreference timePreference,
        TimeOnly? preferredTime,
        SchedulingContext context,
        int maxSlots)
    {
        var results = new List<TimeSlot>();

        if (context.ServiceDuration <= TimeSpan.Zero)
        {
            return results;
        }

        var candidateDate = baseDate.Date;
        var dayWorkingHours = context.WorkingHours.FirstOrDefault();
        if (dayWorkingHours == null)
        {
            return results;
        }

        TimeSpan windowStart = dayWorkingHours.Start;
        TimeSpan windowEnd = dayWorkingHours.End;

        switch (timePreference)
        {
            case TimePreference.Morning: windowEnd = TimeSpan.FromHours(12); break;
            case TimePreference.Afternoon:
                windowStart = TimeSpan.FromHours(12);
                windowEnd = TimeSpan.FromHours(17);
                break;
            case TimePreference.Evening: windowStart = TimeSpan.FromHours(17); break;
        }

        if (preferredTime.HasValue)
        {
            var preferred = preferredTime.Value.ToTimeSpan();
            if (preferred > windowStart)
            {
                windowStart = preferred;
            }

            var preferredEnd = preferred.Add(TimeSpan.FromHours(2));
            if (preferredEnd < windowEnd)
            {
                windowEnd = preferredEnd;
            }
        }

        var currentTime = candidateDate + windowStart;
        var endOfWindow = candidateDate + windowEnd;

        var now = dateTimeProvider.Now;
        if (currentTime < now)
        {
            var minutesSinceMidnight = (long)(now - candidateDate).TotalMinutes;
            var roundedMinutes = ((minutesSinceMidnight + 14) / 15) * 15;
            currentTime = candidateDate.AddMinutes(roundedMinutes);
        }

        while (currentTime + context.ServiceDuration <= endOfWindow &&
               results.Count < maxSlots)
        {
            var candidateSlot = new TimeSlot
            {
                StartUtc = currentTime,
                EndUtc = currentTime + context.ServiceDuration
            };

            bool overlaps = context.ExistingAppointments.Any(a =>
                candidateSlot.StartUtc < a.EndUtc &&
                candidateSlot.EndUtc > a.StartUtc);

            bool inBreak = context.Breaks.Any(b =>
                candidateSlot.StartUtc < b.EndUtc &&
                candidateSlot.EndUtc > b.StartUtc);

            if (!overlaps && !inBreak)
            {
                results.Add(candidateSlot);
                currentTime += context.ServiceDuration;
            }
            else
            {
                currentTime = currentTime.AddMinutes(15);
            }
        }

        return results;
    }
}