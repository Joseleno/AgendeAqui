namespace AgendeAqui.Domain.Abstractions;

public interface INotificationRepository : IRepository<Notifications.Notification>
{
    Task<IReadOnlyList<Notifications.Notification>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default);
    Task<bool> ExistsByAppointmentIdAndTemplateAsync(Guid appointmentId, string templateName, CancellationToken ct = default);
    Task<HashSet<Guid>> GetExistingAppointmentIdsAsync(IEnumerable<Guid> appointmentIds, string templateName, CancellationToken ct = default);
}
