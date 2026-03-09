using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.DataProtection;

public static class DataDeletionRequestErrors
{
    public static readonly Error NotFound = new("DataDeletionRequest.NotFound", "Data deletion request not found.");
    public static readonly Error InvalidTenant = new("DataDeletionRequest.InvalidTenant", "Tenant id is required.");
    public static readonly Error InvalidClient = new("DataDeletionRequest.InvalidClient", "Client id is required.");
}
