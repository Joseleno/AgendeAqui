using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Availability.GetAvailability;

public sealed class GetAvailabilityQueryHandler(
    IScheduleRepository scheduleRepository,
    IAppointmentRepository appointmentRepository,
    IServiceRepository serviceRepository) : IQueryHandler<GetAvailabilityQuery, AvailabilityResponse>
{
    public async ValueTask<Result<AvailabilityResponse>> Handle(
        GetAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(query.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<AvailabilityResponse>(AppointmentErrors.ServiceNotFound);

        var schedule = await scheduleRepository.GetByProfessionalAndDayAsync(
            query.ProfessionalId, query.Date.DayOfWeek, cancellationToken);

        if (schedule is null)
            return Result.Success(new AvailabilityResponse(query.Date, query.ProfessionalId, []));

        var allSlots = schedule.GetAvailableSlots(service.Duration);

        var existingAppointments = await appointmentRepository.GetByProfessionalAndDateAsync(
            query.ProfessionalId, query.Date, cancellationToken);

        var bookedSlots = existingAppointments
            .Where(a => a.Status != Domain.ValueObjects.AppointmentStatus.Cancelled)
            .Select(a => a.TimeSlot)
            .ToList();

        var availableSlots = allSlots
            .Where(slot => !bookedSlots.Any(booked => slot.Overlaps(booked)))
            .Select(slot => new AvailableSlot(slot.Start, slot.End))
            .ToList();

        return Result.Success(new AvailabilityResponse(
            query.Date,
            query.ProfessionalId,
            availableSlots));
    }
}
