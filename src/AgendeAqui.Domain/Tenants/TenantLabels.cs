namespace AgendeAqui.Domain.Tenants;

public sealed class TenantLabels
{
    public string Professional { get; set; } = "Profissional";
    public string Professionals { get; set; } = "Profissionais";
    public string Client { get; set; } = "Cliente";
    public string Clients { get; set; } = "Clientes";
    public string Appointment { get; set; } = "Agendamento";
    public string Appointments { get; set; } = "Agendamentos";
    public string Service { get; set; } = "Serviço";
    public string Services { get; set; } = "Serviços";
    public string Team { get; set; } = "Equipe";
    public string Teams { get; set; } = "Equipes";

    public static TenantLabels Default() => new();
}
