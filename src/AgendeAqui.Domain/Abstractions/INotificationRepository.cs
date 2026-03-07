namespace AgendeAqui.Domain.Abstractions;

public interface INotificationRepository : IRepository<Notifications.Notification>
{
    Task<List<Notifications.Notification>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default);
}
