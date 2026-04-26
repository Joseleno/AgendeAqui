namespace AgendeAqui.Domain.Tenants;

public sealed class TenantFeatures
{
    public bool HasClinicalNotes { get; set; } = true;
    public bool HasTeleconsultation { get; set; } = true;
    public bool HasTeams { get; set; } = true;
    public bool HasPayments { get; set; } = true;

    public static TenantFeatures Default() => new();
}
