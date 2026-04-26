using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Teams;

public sealed class TeamMember : TenantEntity
{
    public Guid TeamId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }

    private TeamMember() { }

    internal static TeamMember Create(Guid tenantId, Guid teamId, Guid professionalId)
    {
        return new TeamMember
        {
            TenantId = tenantId,
            TeamId = teamId,
            ProfessionalId = professionalId,
            JoinedAt = DateTime.UtcNow
        };
    }

    internal void Leave()
    {
        LeftAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive => LeftAt is null;
}
