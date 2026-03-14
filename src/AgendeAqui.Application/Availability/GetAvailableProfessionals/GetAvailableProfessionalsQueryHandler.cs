using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Availability.GetAvailableProfessionals;

public sealed class GetAvailableProfessionalsQueryHandler(
    IProfessionalServiceRepository professionalServiceRepository,
    IProfessionalRepository professionalRepository,
    IServiceRepository serviceRepository,
    IScheduleRepository scheduleRepository,
    IAbsenceRepository absenceRepository,
    IAppointmentRepository appointmentRepository,
    ITenantProvider tenantProvider) : IQueryHandler<GetAvailableProfessionalsQuery, List<ProfessionalAvailabilityResponse>>
{
    public async ValueTask<Result<List<ProfessionalAvailabilityResponse>>> Handle(
        GetAvailableProfessionalsQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var service = await serviceRepository.GetByIdAsync(query.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<List<ProfessionalAvailabilityResponse>>(AppointmentErrors.ServiceNotFound);

        // Get professionals linked to this service; if no links exist, use all active professionals
        var linkedProfessionalIds = await professionalServiceRepository
            .GetProfessionalIdsByServiceAsync(query.ServiceId, cancellationToken);

        var professionals = linkedProfessionalIds.Count > 0
            ? await GetProfessionalsByIdsAsync(linkedProfessionalIds, tenantId, cancellationToken)
            : await professionalRepository.GetActiveByTenantAsync(tenantId, cancellationToken);

        var results = new List<ProfessionalAvailabilityResponse>();

        foreach (var professional in professionals)
        {
            if (!professional.IsActive)
                continue;

            // Check absences — full-day absence means no availability
            var absences = await absenceRepository.GetByProfessionalAndDateAsync(
                professional.Id, query.Date, cancellationToken);

            if (absences.Any(a => a.IsFullDay))
                continue;

            // Get schedule for the day of week
            var schedule = await scheduleRepository.GetByProfessionalAndDayAsync(
                professional.Id, query.Date.DayOfWeek, cancellationToken);

            if (schedule is null)
                continue;

            var allSlots = schedule.GetAvailableSlots(service.Duration);

            // Get existing appointments and filter out cancelled ones
            var existingAppointments = await appointmentRepository.GetByProfessionalAndDateAsync(
                professional.Id, query.Date, cancellationToken);

            var bookedSlots = existingAppointments
                .Where(a => a.Status != Domain.ValueObjects.AppointmentStatus.Cancelled)
                .Select(a => a.TimeSlot)
                .ToList();

            var availableSlotCount = allSlots
                .Where(slot => !bookedSlots.Any(booked => slot.Overlaps(booked)))
                .Where(slot => !absences.Any(a => a.Blocks(slot.Start, slot.End)))
                .Count();

            if (availableSlotCount > 0)
            {
                results.Add(new ProfessionalAvailabilityResponse(
                    professional.Id,
                    professional.Name,
                    professional.Specialty?.Value,
                    availableSlotCount));
            }
        }

        return Result.Success(results.OrderByDescending(r => r.AvailableSlots).ToList());
    }

    private async Task<IReadOnlyList<Domain.Professionals.Professional>> GetProfessionalsByIdsAsync(
        IReadOnlyList<Guid> ids,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var allProfessionals = await professionalRepository.GetActiveByTenantAsync(tenantId, cancellationToken);
        return allProfessionals.Where(p => ids.Contains(p.Id)).ToList();
    }
}
