using AgendeAqui.Domain.Appointments;

namespace AgendeAqui.Application.Abstractions.Notifications;

public interface INotificationService
{
    Task SendAppointmentCreatedAsync(Guid tenantId, Guid appointmentId, CancellationToken ct = default);
    Task SendAppointmentCancelledAsync(Guid tenantId, Guid appointmentId, string reason, CancellationToken ct = default);
    Task SendAppointmentRescheduledAsync(Guid tenantId, Guid appointmentId, DateOnly newDate, CancellationToken ct = default);
    Task SendAppointmentReminderAsync(Guid tenantId, Guid appointmentId, CancellationToken ct = default);
    Task SendAppointmentRemindersAsync(IReadOnlyList<Appointment> appointments, CancellationToken ct = default);
}
