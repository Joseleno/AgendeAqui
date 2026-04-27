namespace AgendeAqui.Application.Tenants.GetTenantUsage;

public sealed record TenantUsageResponse(
    Guid TenantId,
    string Name,
    string Slug,
    string Plan,
    string Status,
    bool IsOnTrial,
    DateTime? TrialEndsAt,
    string? CustomDomain,
    int ProfessionalsCount,
    int MaxProfessionals,
    int ClientsCount,
    int MaxClients,
    int AppointmentsThisMonth,
    int MaxAppointmentsPerMonth);
