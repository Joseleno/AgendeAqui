using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Abstractions;

public interface IPlanLimitChecker
{
    Task<Result> CheckProfessionalLimitAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result> CheckClientLimitAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result> CheckMonthlyAppointmentLimitAsync(Guid tenantId, CancellationToken ct = default);
}
