using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Teams.Events;

namespace AgendeAqui.Domain.Teams;

public sealed class Team : AggregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 500;

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid? LeaderId { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<TeamMember> _members = [];
    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    private Team() { }

    public static Result<Team> Create(Guid tenantId, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Team>(TeamErrors.InvalidName);

        if (name.Length > MaxNameLength)
            return Result.Failure<Team>(TeamErrors.NameTooLong);

        var team = new Team
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };

        team.RaiseDomainEvent(new TeamCreatedEvent(team.Id, tenantId));

        return Result.Success(team);
    }

    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TeamErrors.InvalidName);

        if (name.Length > MaxNameLength)
            return Result.Failure(TeamErrors.NameTooLong);

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddMember(Guid professionalId, Guid professionalTenantId)
    {
        if (professionalTenantId != TenantId)
            return Result.Failure(TeamErrors.CrossTenantMember);

        if (_members.Any(m => m.ProfessionalId == professionalId && m.IsActive))
            return Result.Failure(TeamErrors.MemberAlreadyExists);

        var member = TeamMember.Create(TenantId, Id, professionalId);
        _members.Add(member);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TeamMemberAddedEvent(Id, professionalId));

        return Result.Success();
    }

    public Result RemoveMember(Guid professionalId)
    {
        var member = _members.FirstOrDefault(m => m.ProfessionalId == professionalId && m.IsActive);
        if (member is null)
            return Result.Failure(TeamErrors.MemberNotFound);

        // If this professional is the leader, the caller must clear the leader first
        if (LeaderId == professionalId)
            return Result.Failure(TeamErrors.CannotRemoveLeader);

        member.Leave();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TeamMemberRemovedEvent(Id, professionalId));

        return Result.Success();
    }

    public Result AssignLeader(Guid professionalId)
    {
        if (!_members.Any(m => m.ProfessionalId == professionalId && m.IsActive))
            return Result.Failure(TeamErrors.LeaderMustBeMember);

        LeaderId = professionalId;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TeamLeaderChangedEvent(Id, professionalId));

        return Result.Success();
    }

    public Result ClearLeader()
    {
        LeaderId = null;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TeamLeaderChangedEvent(Id, null));

        return Result.Success();
    }

    public void Disband()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TeamDisbandedEvent(Id, TenantId));
    }
}
