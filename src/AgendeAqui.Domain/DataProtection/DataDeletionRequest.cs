using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.DataProtection;

public sealed class DataDeletionRequest : TenantEntity
{
    public Guid ClientId { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DeletionRequestStatus Status { get; private set; }

    private DataDeletionRequest() { }

    public static Result<DataDeletionRequest> Create(Guid tenantId, Guid clientId)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<DataDeletionRequest>(DataDeletionRequestErrors.InvalidTenant);

        if (clientId == Guid.Empty)
            return Result.Failure<DataDeletionRequest>(DataDeletionRequestErrors.InvalidClient);

        var request = new DataDeletionRequest
        {
            TenantId = tenantId,
            ClientId = clientId,
            RequestedAt = DateTime.UtcNow,
            Status = DeletionRequestStatus.Pending
        };

        return Result.Success(request);
    }

    public void Complete()
    {
        if (Status == DeletionRequestStatus.Completed)
            return;

        var now = DateTime.UtcNow;
        CompletedAt = now;
        Status = DeletionRequestStatus.Completed;
        UpdatedAt = now;
    }
}
