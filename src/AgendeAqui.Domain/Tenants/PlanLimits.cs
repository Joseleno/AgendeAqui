namespace AgendeAqui.Domain.Tenants;

public sealed record PlanLimits(
    int MaxProfessionals,
    int MaxClients,
    int MaxAppointmentsPerMonth,
    bool CanUseApiKey,
    bool CanUseWebhooks,
    bool CanExportData)
{
    public static PlanLimits For(TenantPlan plan) => plan switch
    {
        TenantPlan.Free         => new(2,   50,   100,  false, false, false),
        TenantPlan.Starter      => new(5,   200,  500,  true,  false, true),
        TenantPlan.Professional => new(20,  2000, 5000, true,  true,  true),
        TenantPlan.Enterprise   => new(int.MaxValue, int.MaxValue, int.MaxValue, true, true, true),
        _                       => new(2,   50,   100,  false, false, false)
    };

    public bool IsWithinProfessionalLimit(int current) => current < MaxProfessionals;
    public bool IsWithinClientLimit(int current) => current < MaxClients;
    public bool IsWithinMonthlyAppointmentLimit(int current) => current < MaxAppointmentsPerMonth;
}
