using AgendeAqui.Domain.Appointments.Events;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Domain.Appointments;

public sealed class Appointment : AggregateRoot
{
    public Guid ProfessionalId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid ClientId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeSlot TimeSlot { get; private set; } = default!;
    public AppointmentStatus Status { get; private set; } = default!;
    public string? Notes { get; private set; }
    public string? ExternalId { get; private set; }
    public bool IsTeleconsultation { get; private set; }
    public string? MeetingUrl { get; private set; }

    private Appointment() { }

    public static Result<Appointment> Create(
        Guid tenantId,
        Guid professionalId,
        Guid serviceId,
        Guid clientId,
        DateOnly date,
        TimeSlot timeSlot,
        string? notes = null,
        Guid? sourceApiKeyId = null)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Appointment>(AppointmentErrors.InvalidTenant);
        if (professionalId == Guid.Empty)
            return Result.Failure<Appointment>(AppointmentErrors.ProfessionalNotFound);
        if (serviceId == Guid.Empty)
            return Result.Failure<Appointment>(AppointmentErrors.ServiceNotFound);
        if (clientId == Guid.Empty)
            return Result.Failure<Appointment>(AppointmentErrors.ClientNotFound);

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

        appointment.RaiseDomainEvent(new AppointmentCreatedEvent(appointment.Id, sourceApiKeyId));

        return Result.Success(appointment);
    }

    public Result Cancel(string reason, Guid? sourceApiKeyId = null)
    {
        if (!Status.CanTransitionTo(AppointmentStatus.Cancelled))
            return Result.Failure(AppointmentErrors.InvalidTransition);

        Status = AppointmentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppointmentCancelledEvent(Id, reason, sourceApiKeyId));

        return Result.Success();
    }

    public Result Reschedule(DateOnly newDate, TimeSlot newTimeSlot, Guid? sourceApiKeyId = null)
    {
        if (!Status.CanTransitionTo(AppointmentStatus.Scheduled))
            return Result.Failure(AppointmentErrors.InvalidTransition);

        Date = newDate;
        TimeSlot = newTimeSlot;
        Status = AppointmentStatus.Scheduled;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppointmentRescheduledEvent(Id, newDate, sourceApiKeyId));

        return Result.Success();
    }

    public Result Confirm()
    {
        if (!Status.CanTransitionTo(AppointmentStatus.Confirmed))
            return Result.Failure(AppointmentErrors.InvalidTransition);

        Status = AppointmentStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppointmentConfirmedEvent(Id));

        return Result.Success();
    }

    public Result Start()
    {
        if (!Status.CanTransitionTo(AppointmentStatus.InProgress))
            return Result.Failure(AppointmentErrors.InvalidTransition);

        Status = AppointmentStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppointmentStartedEvent(Id));

        return Result.Success();
    }

    public Result MarkNoShow()
    {
        if (!Status.CanTransitionTo(AppointmentStatus.NoShow))
            return Result.Failure(AppointmentErrors.InvalidTransition);

        Status = AppointmentStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppointmentNoShowEvent(Id));

        return Result.Success();
    }

    public Result Complete()
    {
        if (!Status.CanTransitionTo(AppointmentStatus.Completed))
            return Result.Failure(AppointmentErrors.InvalidTransition);

        Status = AppointmentStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppointmentCompletedEvent(Id));

        return Result.Success();
    }

    public void SetExternalId(string externalId)
    {
        ExternalId = externalId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTeleconsultation(bool isTeleconsultation)
    {
        IsTeleconsultation = isTeleconsultation;
        UpdatedAt = DateTime.UtcNow;
    }

    public void GenerateMeetingLink()
    {
        if (!IsTeleconsultation)
            return;

        MeetingUrl = $"https://meet.jit.si/agendeaqui-{TenantId:N}-{Guid.NewGuid():N}";
        UpdatedAt = DateTime.UtcNow;
    }
}
