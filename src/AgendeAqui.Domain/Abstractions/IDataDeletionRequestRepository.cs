namespace AgendeAqui.Domain.Abstractions;

public interface IDataDeletionRequestRepository : IRepository<DataProtection.DataDeletionRequest>
{
    Task<DataProtection.DataDeletionRequest?> GetPendingByClientIdAsync(Guid clientId, CancellationToken ct = default);
}
