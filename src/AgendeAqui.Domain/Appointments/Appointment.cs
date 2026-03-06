using AgendeAqui.Domain.Appointments.Events;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Domain.Appointments;

public sealed class Appointment : AggregateRoot
{
    public static readonly Error InvalidTransition = new("Appointment.InvalidTransition", "The requested status transition is not allowed.");

    public Guid ProfessionalId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid ClientId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeSlot TimeSlot { get; private set; } = default!;
    public AppointmentStatus Status { get; private set; } = default!;
    public string? Notes { get; private set; }

    private Appointment() { }

    public static Appointment Create(
        Guid tenantId,
        Guid professionalId,
        Guid serviceId,
        Guid clientId,
        DateOnly date,
        TimeSlot timeSlot,
        string? notes = null)
    {
        var appointment = new Appointment
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            ServiceId = serviceId,
            ClientId = clientId,
            Date = date,
            TimeSlot = timeSlot,
            Status = AppointmentStatus.Scheduled,
            Notes = notes
        };

        appointment.RaiseDomainEvent(new AppointmentCreatedEvent(appointment.Id));

        return appointment;
    }

    public Result Cancel(string reason)
    {
        if (!Status.CanTransitionTo(AppointmentStatus.Cancelled))
            return Result.Failure(InvalidTransition);

        Status = AppointmentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppointmentCancelledEvent(Id, reason));

        return Result.Success();
    }

    public Result Reschedule(DateOnly newDate, TimeSlot newTimeSlot)
    {
        // Rescheduling is allowed from Scheduled or Confirmed states
        if (Status != AppointmentStatus.Scheduled && Status != AppointmentStatus.Confirmed)
            return Result.Failure(InvalidTransition);

        Date = newDate;
        TimeSlot = newTimeSlot;
        Status = AppointmentStatus.Scheduled;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppointmentRescheduledEvent(Id, newDate));

        return Result.Success();
    }

    public Result Confirm()
    {
        if (!Status.CanTransitionTo(AppointmentStatus.Confirmed))
            return Result.Failure(InvalidTransition);

        Status = AppointmentStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Start()
    {
        if (!Status.CanTransitionTo(AppointmentStatus.InProgress))
            return Result.Failure(InvalidTransition);

        Status = AppointmentStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkNoShow()
    {
        if (!Status.CanTransitionTo(AppointmentStatus.NoShow))
            return Result.Failure(InvalidTransition);

        Status = AppointmentStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Complete()
    {
        if (!Status.CanTransitionTo(AppointmentStatus.Completed))
            return Result.Failure(InvalidTransition);

        Status = AppointmentStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
