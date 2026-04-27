using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.ChangeTenantPlan;

public sealed record ChangeTenantPlanCommand(Guid TenantId, string Plan) : ICommand;
